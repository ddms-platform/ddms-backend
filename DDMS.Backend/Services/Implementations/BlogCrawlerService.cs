using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.DTOs.Blog;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

/// <summary>
/// Cào tin du lịch Đà Nẵng từ RSS rồi nhờ Gemini viết lại.
///
/// Hai điều cố ý làm:
///   1. Đọc RSS chứ không cào HTML. RSS là kênh các báo chủ động phát hành để
///      tổng hợp, không phải lách chặn bot, và cấu trúc ổn định hơn nhiều.
///   2. Bài tạo ra luôn ở trạng thái draft. Nội dung AI sinh phải có người đọc
///      lại trước khi lên trang — mô tả trong RSS chỉ vài dòng nên khi viết dài
///      ra, AI có thể bịa chi tiết.
/// </summary>
public class BlogCrawlerService : IBlogCrawlerService
{
    private readonly IBlogRepository _repo;
    private readonly IGeminiTextGenerator _gemini;
    private readonly BlogCrawlerOptions _options;
    private readonly HttpClient _http;
    private readonly ILogger<BlogCrawlerService> _logger;

    public BlogCrawlerService(
        IBlogRepository repo,
        IGeminiTextGenerator gemini,
        IOptions<BlogCrawlerOptions> options,
        HttpClient http,
        ILogger<BlogCrawlerService> logger)
    {
        _repo = repo;
        _gemini = gemini;
        _options = options.Value;
        _http = http;
        _logger = logger;
    }

    public async Task<CrawlResult> CrawlAsync(CancellationToken ct)
    {
        var result = new CrawlResult();
        var seenInThisRun = new HashSet<string>();

        foreach (var query in _options.Queries)
        {
            if (result.Created >= _options.MaxItemsPerRun) break;

            List<FeedItem> items;
            try
            {
                items = await FetchFeedAsync(query, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không đọc được RSS cho truy vấn {Query}", query);
                result.Errors.Add($"RSS lỗi ({query}): {ex.Message}");
                continue;
            }

            result.Fetched += items.Count;

            foreach (var item in items)
            {
                if (result.Created >= _options.MaxItemsPerRun) break;

                var hash = Sha256(item.Link);
                if (!seenInThisRun.Add(hash)) { result.Skipped++; continue; }

                if (item.PublishedAt is { } pub
                    && pub < DateTime.UtcNow.AddDays(-_options.MaxAgeDays))
                {
                    result.Skipped++;
                    continue;
                }

                if (await _repo.ExistsBySourceHashAsync(hash, ct)) { result.Skipped++; continue; }

                try
                {
                    var post = await BuildPostAsync(item, hash, ct);
                    if (post == null) { result.Skipped++; continue; }

                    _repo.Add(post);
                    await _repo.SaveChangesAsync(ct);
                    result.Created++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Viết lại thất bại cho bài {Title}", item.Title);
                    result.Errors.Add($"Viết lại lỗi ({Trim(item.Title, 40)}): {ex.Message}");
                }
            }
        }

        return result;
    }

    // ------------------------------------------------------------------ RSS

    private sealed record FeedItem(
        string Title, string Link, string Description, string SourceName, DateTime? PublishedAt);

    private async Task<List<FeedItem>> FetchFeedAsync(string query, CancellationToken ct)
    {
        var url = "https://news.google.com/rss/search"
                + $"?q={Uri.EscapeDataString(query)}&hl=vi&gl=VN&ceid=VN:vi";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.UserAgent.ParseAdd("DDMS-NewsBot/1.0 (+https://ddms.vn)");
        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var xml = await res.Content.ReadAsStringAsync(ct);
        var doc = XDocument.Parse(xml);

        var list = new List<FeedItem>();
        foreach (var el in doc.Descendants("item"))
        {
            var rawTitle = (string?)el.Element("title") ?? string.Empty;
            var link = (string?)el.Element("link") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(link)) continue;

            // Google News gắn " - Tên báo" vào cuối tiêu đề; tách ra làm nguồn.
            var sourceEl = el.Elements().FirstOrDefault(x => x.Name.LocalName == "source");
            var source = sourceEl?.Value;
            var title = rawTitle;
            if (string.IsNullOrWhiteSpace(source))
            {
                var idx = rawTitle.LastIndexOf(" - ", StringComparison.Ordinal);
                if (idx > 0)
                {
                    source = rawTitle[(idx + 3)..].Trim();
                    title = rawTitle[..idx].Trim();
                }
            }
            else if (rawTitle.EndsWith(" - " + source, StringComparison.Ordinal))
            {
                title = rawTitle[..^(source.Length + 3)].Trim();
            }

            DateTime? published = null;
            if (DateTime.TryParse((string?)el.Element("pubDate"), CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out var parsed))
                published = parsed;

            list.Add(new FeedItem(
                title,
                link,
                StripHtml((string?)el.Element("description") ?? string.Empty),
                string.IsNullOrWhiteSpace(source) ? "Google News" : source!,
                published));
        }

        return list;
    }

    // ------------------------------------------------------------------ AI

    private async Task<blog_post?> BuildPostAsync(FeedItem item, string hash, CancellationToken ct)
    {
        var prompt = $$"""
Bạn là biên tập viên của DDMS — nền tảng đặt tour du thuyền tại Đà Nẵng.

Dưới đây là tiêu đề và đoạn mô tả ngắn của một bản tin du lịch. Hãy viết lại
thành một bài ngắn cho mục tin tức của DDMS.

Tiêu đề gốc: {{item.Title}}
Nguồn: {{item.SourceName}}
Mô tả: {{item.Description}}

Yêu cầu bắt buộc:
- Viết tiếng Việt có dấu, giọng tự nhiên, không sáo rỗng.
- CHỈ dùng thông tin có trong tiêu đề và mô tả ở trên. Không thêm số liệu,
  ngày tháng, tên người hay chi tiết nào không có sẵn. Thiếu thông tin thì
  viết ngắn lại, tuyệt đối không bịa.
- Độ dài 3 đến 4 đoạn, mỗi đoạn 2-3 câu.
- Không nhắc lại tên báo trong nội dung, phần dẫn nguồn hệ thống tự thêm.

Trả về DUY NHẤT một object JSON, không kèm giải thích, không rào ```json:
{
  "title": "tiêu đề mới, dưới 100 ký tự",
  "summary": "tóm tắt 1-2 câu, dưới 250 ký tự",
  "category": "một trong: cam_nang, kinh_nghiem, tin_tuc",
  "content": "nội dung Markdown, các đoạn cách nhau bằng hai xuống dòng",
  "video_scenes": [
    {"narration": "lời đọc cảnh 1, 1-2 câu", "caption": "chữ hiện trên màn hình, dưới 60 ký tự"},
    {"narration": "...", "caption": "..."}
  ]
}
video_scenes gồm 4 đến 5 cảnh, tổng lời đọc khoảng 45 giây.
""";

        var raw = await _gemini.GenerateTextAsync(prompt, ct);
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var json = ExtractJson(raw);
        if (json == null)
        {
            _logger.LogWarning("Gemini trả về không phải JSON cho bài {Title}", item.Title);
            return null;
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var title = Trim(GetString(root, "title") ?? item.Title, 290)!;
        var scenes = new List<VideoScene>();
        if (root.TryGetProperty("video_scenes", out var sc) && sc.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in sc.EnumerateArray())
            {
                var narration = GetString(s, "narration");
                if (string.IsNullOrWhiteSpace(narration)) continue;
                scenes.Add(new VideoScene
                {
                    Narration = narration!,
                    Caption = GetString(s, "caption"),
                    DurationSeconds = 6,
                });
            }
        }

        var category = GetString(root, "category") ?? "tin_tuc";
        if (category is not ("cam_nang" or "kinh_nghiem" or "tin_tuc")) category = "tin_tuc";

        return new blog_post
        {
            id = Guid.NewGuid(),
            title = title,
            slug = await UniqueSlugAsync(title, ct),
            summary = Trim(GetString(root, "summary"), 590),
            content = GetString(root, "content"),
            category = category,
            status = "draft",
            source_name = Trim(item.SourceName, 140),
            source_url = Trim(item.Link, 990),
            source_published_at = item.PublishedAt,
            source_hash = hash,
            video_script = scenes.Count > 0 ? JsonSerializer.Serialize(scenes) : null,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow,
        };
    }

    // -------------------------------------------------------------- Helpers

    private async Task<string> UniqueSlugAsync(string title, CancellationToken ct)
    {
        var baseSlug = Slugify(title);
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "bai-viet";

        var slug = baseSlug;
        var i = 2;
        while (await _repo.SlugExistsAsync(slug, ct))
        {
            slug = $"{baseSlug}-{i++}";
            if (i > 50) { slug = $"{baseSlug}-{Guid.NewGuid():N}"[..60]; break; }
        }
        return slug;
    }

    /// <summary>Bỏ dấu tiếng Việt rồi chuyển thành slug.</summary>
    internal static string Slugify(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(c);
        }
        var s = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        s = s.Replace('đ', 'd');
        s = Regex.Replace(s, @"[^a-z0-9]+", "-");
        return Regex.Replace(s, @"^-+|-+$", "");
    }

    private static string StripHtml(string html) =>
        Regex.Replace(Regex.Replace(html, "<.*?>", " "), @"\s+", " ").Trim();

    /// <summary>Gemini hay bọc JSON trong ```; cắt lấy phần trong ngoặc nhọn.</summary>
    private static string? ExtractJson(string raw)
    {
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        return start >= 0 && end > start ? raw[start..(end + 1)] : null;
    }

    private static string? GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    /// <summary>Cắt bớt cho vừa độ dài cột trong DB.</summary>
    private static string? Trim(string? v, int max) =>
        v is null ? null : (v.Length <= max ? v : v[..max]);

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
