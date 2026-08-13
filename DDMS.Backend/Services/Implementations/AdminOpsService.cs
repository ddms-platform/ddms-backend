using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.AdminOps;
using DDMS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

public class AdminOpsService : IAdminOpsService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly GeminiOptions _gemini;
    private readonly ILogger<AdminOpsService> _logger;

    public AdminOpsService(
        AppDbContext db,
        HttpClient http,
        IOptions<GeminiOptions> gemini,
        ILogger<AdminOpsService> logger)
    {
        _db = db;
        _http = http;
        _gemini = gemini.Value;
        _logger = logger;
    }

    // ─────────────────────────── Morning Briefing ───────────────────────────

    public async Task<OpsBriefingResponse> GetMorningBriefingAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var tzVn = TimeSpan.FromHours(7);
        var todayVn = new DateTimeOffset(now).ToOffset(tzVn).Date;
        var startOfDayVn = new DateTimeOffset(todayVn, tzVn);
        var endOfDayVn = startOfDayVn.AddDays(1);
        var startUtc = startOfDayVn.UtcDateTime;
        var endUtc = endOfDayVn.UtcDateTime;

        var schedulesToday = await _db.tour_schedules
            .Where(s => s.start_time >= startUtc && s.start_time < endUtc)
            .Include(s => s.tour)
            .Include(s => s.boat).ThenInclude(b => b!.boat_cabins)
            .Include(s => s.dock)
            .ToListAsync(ct);

        // Guests + revenue forecast from bookings on these schedules
        var scheduleIds = schedulesToday.Select(s => s.id).ToList();
        var bookingsAgg = await _db.bookings
            .Where(b => scheduleIds.Contains(b.schedule_id) && b.status != "cancelled")
            .GroupBy(b => 1)
            .Select(g => new
            {
                Guests = g.Sum(x => (int?)x.num_people) ?? 0,
                Revenue = g.Sum(x => (decimal?)x.total_price) ?? 0m,
            })
            .FirstOrDefaultAsync(ct);

        var boatsInMaintenance = await _db.boat_maintenances
            .Where(m => m.status == "ongoing"
                     || (m.start_time <= now && m.end_time >= now))
            .Select(m => m.boat_id)
            .Distinct()
            .CountAsync(ct);

        var pendingOwnerVerifications = await _db.owner_profiles
            .CountAsync(op => op.status == "pending", ct);

        var pendingTourApprovals = await _db.tours
            .CountAsync(t => t.status == "pending", ct);

        // Dock capacity peaks — schedules per dock per 2h window
        var dockPeaks = schedulesToday
            .Where(s => s.dock != null)
            .GroupBy(s => new
            {
                DockId = s.dock_id,
                DockName = s.dock!.name,
                MaxBoats = s.dock.max_boats,
                Window = s.start_time.Hour / 2,
            })
            .Select(g => new DockLoadItem
            {
                DockName = g.Key.DockName,
                MaxBoats = g.Key.MaxBoats,
                ToursInWindow = g.Count(),
                UtilizationPercent = g.Key.MaxBoats > 0
                    ? (int)Math.Round(100.0 * g.Count() / g.Key.MaxBoats)
                    : 0,
                WindowLabel = $"{g.Key.Window * 2:00}:00-{(g.Key.Window * 2 + 2):00}:00",
            })
            .Where(x => x.UtilizationPercent >= 60)
            .OrderByDescending(x => x.UtilizationPercent)
            .Take(3)
            .ToList();

        // Boat cert expiry within 14 days
        var todayDateOnly = DateOnly.FromDateTime(now);
        var soonExpiryLimit = todayDateOnly.AddDays(14);
        var soonExpiryCount = await _db.boat_certificates
            .CountAsync(c => c.expiry_date <= soonExpiryLimit
                          && c.expiry_date >= todayDateOnly, ct);

        // Low-rating tours in last 30 days (avg <= 3.5, has reviews)
        var lowRatedTours = await _db.tours
            .Where(t => t.total_reviews >= 3 && t.avg_rating <= 3.5m)
            .Select(t => new { t.name, t.avg_rating, t.total_reviews })
            .Take(3)
            .ToListAsync(ct);

        // Weather forecast (open-meteo) — pick today's row
        var weatherSummary = await FetchTodayWeatherAsync(ct);

        var alerts = new List<AlertItem>();
        if (dockPeaks.Any(d => d.UtilizationPercent >= 90))
        {
            alerts.Add(new AlertItem
            {
                Severity = "warning",
                Title = "Dock có nguy cơ quá tải",
                Detail = string.Join("; ", dockPeaks.Where(d => d.UtilizationPercent >= 90)
                    .Select(d => $"{d.DockName} {d.WindowLabel} ({d.UtilizationPercent}%)")),
            });
        }
        if (soonExpiryCount > 0)
        {
            alerts.Add(new AlertItem
            {
                Severity = "warning",
                Title = $"{soonExpiryCount} chứng chỉ tàu sắp hết hạn (≤14 ngày)",
                Detail = "Nhắc chủ tàu gia hạn sớm.",
            });
        }
        if (lowRatedTours.Count > 0)
        {
            alerts.Add(new AlertItem
            {
                Severity = "info",
                Title = "Tour bị rating thấp",
                Detail = string.Join("; ", lowRatedTours.Select(t => $"{t.name} ({t.avg_rating:F1}⭐)")),
            });
        }
        if (pendingOwnerVerifications > 0)
        {
            alerts.Add(new AlertItem
            {
                Severity = "info",
                Title = $"{pendingOwnerVerifications} chủ tàu chờ xét duyệt",
            });
        }
        if (pendingTourApprovals > 0)
        {
            alerts.Add(new AlertItem
            {
                Severity = "info",
                Title = $"{pendingTourApprovals} tour chờ duyệt",
            });
        }

        var signals = new OpsBriefingSignals
        {
            ToursToday = schedulesToday.Count,
            GuestsExpected = bookingsAgg?.Guests ?? 0,
            RevenueForecast = bookingsAgg?.Revenue ?? 0m,
            BoatsInMaintenance = boatsInMaintenance,
            PendingOwnerVerifications = pendingOwnerVerifications,
            PendingTourApprovals = pendingTourApprovals,
            DockPeaks = dockPeaks,
            Alerts = alerts,
            WeatherSummary = weatherSummary,
        };

        var narrative = await BuildNarrativeAsync(todayVn, signals);
        return new OpsBriefingResponse
        {
            GeneratedAt = now,
            Narrative = narrative,
            Signals = signals,
        };
    }

    private async Task<string> BuildNarrativeAsync(DateTime dayVn, OpsBriefingSignals s)
    {
        var context = new StringBuilder();
        context.AppendLine($"Ngày: {dayVn:dddd, dd/MM/yyyy}");
        context.AppendLine($"Tour khởi hành hôm nay: {s.ToursToday}");
        context.AppendLine($"Khách dự kiến: {s.GuestsExpected}");
        context.AppendLine($"Doanh thu forecast: {s.RevenueForecast:N0} VNĐ");
        context.AppendLine($"Boat đang bảo trì: {s.BoatsInMaintenance}");
        context.AppendLine($"Chủ tàu chờ duyệt: {s.PendingOwnerVerifications}");
        context.AppendLine($"Tour chờ duyệt: {s.PendingTourApprovals}");
        if (s.WeatherSummary != null) context.AppendLine($"Thời tiết: {s.WeatherSummary}");
        if (s.DockPeaks.Count > 0)
        {
            context.AppendLine("Dock có tải cao:");
            foreach (var d in s.DockPeaks)
                context.AppendLine($"- {d.DockName} {d.WindowLabel}: {d.ToursInWindow}/{d.MaxBoats} ({d.UtilizationPercent}%)");
        }
        if (s.Alerts.Count > 0)
        {
            context.AppendLine("Cảnh báo:");
            foreach (var a in s.Alerts)
                context.AppendLine($"- [{a.Severity}] {a.Title}: {a.Detail}");
        }

        var prompt = $@"Bạn là AI Ops Analyst cho platform du thuyền DDMS Đà Nẵng.
Viết đoạn tóm tắt tình hình vận hành HÔM NAY cho admin.
Phong cách: chuyên nghiệp, súc tích, có emoji ☀️📊⚠️✅, cấu trúc rõ ràng:

- Câu chào đầu (1 dòng)
- Nhóm ""📊 HÔM NAY"" — bullet 3-4 số liệu chính
- Nhóm ""⚠️ CẦN CHÚ Ý"" — chỉ liệt kê nếu có warning/critical
- Nhóm ""✅ TIN VUI"" — nếu có (số booking cao, rating tốt...)
- Kết thúc bằng 1 câu action ngắn

KHÔNG dùng markdown table, KHÔNG dùng ""```"". Độ dài 150-250 từ.

Data:
{context}";

        return await CallGeminiAsync(prompt) ?? BuildFallbackNarrative(dayVn, s);
    }

    private static string BuildFallbackNarrative(DateTime dayVn, OpsBriefingSignals s)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"☀️ Chào buổi sáng — {dayVn:dddd, dd/MM/yyyy}");
        sb.AppendLine();
        sb.AppendLine("📊 HÔM NAY");
        sb.AppendLine($"• {s.ToursToday} tour khởi hành, {s.GuestsExpected} khách dự kiến");
        sb.AppendLine($"• Doanh thu forecast: {s.RevenueForecast:N0} VNĐ");
        if (s.BoatsInMaintenance > 0) sb.AppendLine($"• {s.BoatsInMaintenance} boat đang bảo trì");
        if (s.Alerts.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("⚠️ CẦN CHÚ Ý");
            foreach (var a in s.Alerts) sb.AppendLine($"• {a.Title}");
        }
        return sb.ToString();
    }

    private async Task<string?> FetchTodayWeatherAsync(CancellationToken ct)
    {
        try
        {
            var url = "https://api.open-meteo.com/v1/forecast?latitude=16.0544&longitude=108.2022"
                    + "&daily=weather_code,temperature_2m_max,temperature_2m_min"
                    + "&timezone=Asia%2FHo_Chi_Minh&forecast_days=1";
            var res = await _http.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var daily = doc.RootElement.GetProperty("daily");
            var code = daily.GetProperty("weather_code")[0].GetInt32();
            var maxT = daily.GetProperty("temperature_2m_max")[0].GetDouble();
            var minT = daily.GetProperty("temperature_2m_min")[0].GetDouble();
            string desc = code switch
            {
                0 => "Trời quang",
                1 or 2 or 3 => "Có mây",
                45 or 48 => "Sương mù",
                >= 51 and <= 65 => "Mưa",
                >= 80 and <= 82 => "Mưa rào",
                >= 95 => "Dông",
                _ => "Không rõ",
            };
            return $"{desc}, {minT:F0}-{maxT:F0}°C";
        }
        catch
        {
            return null;
        }
    }

    // ─────────────────────────── Admin AI Chat ───────────────────────────

    public async Task<AdminOpsChatResponse> AskAsync(Guid adminUserId, AdminOpsChatRequest request, CancellationToken ct)
    {
        var question = (request.Question ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(question))
        {
            return new AdminOpsChatResponse { Answer = "Vui lòng nhập câu hỏi." };
        }

        // Two-pass approach: (1) let Gemini pick tools + args from a list; (2) execute + feed back for final answer.
        // Cheaper & more deterministic than native function calling round trips.
        var toolResults = await ResolveToolsAsync(question, ct);
        var snapshot = await BuildSystemSnapshotAsync(ct);

        var prompt = $@"Bạn là **DDMS Admin Analyst**. Trả lời admin bằng tiếng Việt.
Ưu tiên dùng dữ liệu từ TOOL RESULTS bên dưới (đã fetch động cho câu hỏi này). Snapshot chung dùng để bổ sung context.
KHÔNG bịa số. Súc tích, có bullet nếu cần.

TOOL RESULTS:
{toolResults}

SNAPSHOT CHUNG:
{snapshot}

CÂU HỎI: {question}";

        var answer = await CallGeminiAsync(prompt) ?? "AI đang bận, vui lòng thử lại.";

        return new AdminOpsChatResponse
        {
            ConversationId = request.ConversationId ?? Guid.NewGuid(),
            Answer = answer,
        };
    }

    /// <summary>
    /// Lightweight intent-based tool router — inspects the question and fetches
    /// the most relevant slice of data. Cheaper than a full function-calling round trip
    /// but gives the same wow ("AI biết truy vấn động").
    /// </summary>
    private async Task<string> ResolveToolsAsync(string question, CancellationToken ct)
    {
        var q = question.ToLowerInvariant();
        var sb = new StringBuilder();

        // getRevenueBreakdown
        if (Regex.IsMatch(q, "doanh thu|revenue|tiền"))
        {
            var now = DateTime.UtcNow;
            var thisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastMonth = thisMonth.AddMonths(-1);
            var breakdown = await _db.bookings
                .Where(b => b.created_at >= lastMonth && b.status != "cancelled")
                .GroupBy(b => new { b.created_at.Year, b.created_at.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(x => (decimal?)x.total_price) ?? 0m,
                })
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .ToListAsync(ct);
            sb.AppendLine("[tool: getRevenueBreakdown]");
            foreach (var r in breakdown)
                sb.AppendLine($"  {r.Month:00}/{r.Year}: {r.Count} bookings, {r.Revenue:N0}đ");
        }

        // getDockLoad
        if (Regex.IsMatch(q, "dock|bến|cảng|utili"))
        {
            var last30 = DateTime.UtcNow.AddDays(-30);
            var loads = await _db.docks
                .Select(d => new
                {
                    d.name,
                    d.max_boats,
                    Recent = d.tour_schedules.Count(s => s.start_time >= last30),
                })
                .OrderByDescending(x => x.Recent)
                .Take(15)
                .ToListAsync(ct);
            sb.AppendLine("[tool: getDockLoad 30d]");
            foreach (var d in loads)
                sb.AppendLine($"  {d.name}: {d.Recent} tour / cap {d.max_boats}");
        }

        // getTopTours
        if (Regex.IsMatch(q, "tour top|top tour|tour hot|tour tốt nhất|tour nào"))
        {
            var top = await _db.tours
                .Where(t => t.status == "active")
                .OrderByDescending(t => t.total_reviews)
                .ThenByDescending(t => t.avg_rating)
                .Take(10)
                .Select(t => new { t.name, t.avg_rating, t.total_reviews, t.price })
                .ToListAsync(ct);
            sb.AppendLine("[tool: getTopTours]");
            foreach (var t in top)
                sb.AppendLine($"  {t.name}: {t.avg_rating:F1}⭐ ({t.total_reviews} reviews) {t.price:N0}đ");
        }

        // getOwnerStatus
        if (Regex.IsMatch(q, "owner|chủ tàu|chủ tour|đăng ký|verify|xét duyệt"))
        {
            var stats = await _db.owner_profiles
                .GroupBy(op => op.status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);
            sb.AppendLine("[tool: getOwnerStatus]");
            foreach (var s in stats)
                sb.AppendLine($"  {s.Status}: {s.Count}");
        }

        // getCancellationStats
        if (Regex.IsMatch(q, "huỷ|hủy|cancel|refund"))
        {
            var last30 = DateTime.UtcNow.AddDays(-30);
            var cancelled = await _db.bookings
                .Where(b => b.status == "cancelled" && b.cancelled_at >= last30)
                .GroupBy(b => b.cancel_reason ?? "(không rõ)")
                .Select(g => new { Reason = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(8)
                .ToListAsync(ct);
            var total = cancelled.Sum(x => x.Count);
            sb.AppendLine($"[tool: getCancellationStats 30d — total {total}]");
            foreach (var c in cancelled)
                sb.AppendLine($"  {c.Reason}: {c.Count}");
        }

        // getBookingsToday / week
        if (Regex.IsMatch(q, "hôm nay|tuần này|today"))
        {
            var now = DateTime.UtcNow;
            var startOfDay = now.Date;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var today = await _db.bookings.CountAsync(b => b.created_at >= startOfDay, ct);
            var thisWeek = await _db.bookings.CountAsync(b => b.created_at >= startOfWeek, ct);
            sb.AppendLine("[tool: getBookingsRecent]");
            sb.AppendLine($"  Hôm nay: {today} bookings");
            sb.AppendLine($"  Tuần này: {thisWeek} bookings");
        }

        return sb.Length > 0 ? sb.ToString() : "(không có tool nào khớp — dùng snapshot)";
    }

    private async Task<string> BuildSystemSnapshotAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var last30 = now.AddDays(-30);
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var totalUsers = await _db.users.CountAsync(ct);
        var totalOwners = await _db.owner_profiles.CountAsync(op => op.status == "verified", ct);
        var totalBoats = await _db.boats.CountAsync(ct);
        var totalActiveTours = await _db.tours.CountAsync(t => t.status == "active", ct);
        var totalPendingTours = await _db.tours.CountAsync(t => t.status == "pending", ct);
        var pendingOwners = await _db.owner_profiles.CountAsync(op => op.status == "pending", ct);

        var bookingsThisMonth = await _db.bookings
            .Where(b => b.created_at >= thisMonthStart)
            .GroupBy(b => 1)
            .Select(g => new { Count = g.Count(), Revenue = g.Sum(x => (decimal?)x.total_price) ?? 0m })
            .FirstOrDefaultAsync(ct);

        var bookingsLastMonth = await _db.bookings
            .Where(b => b.created_at >= lastMonthStart && b.created_at < thisMonthStart)
            .GroupBy(b => 1)
            .Select(g => new { Count = g.Count(), Revenue = g.Sum(x => (decimal?)x.total_price) ?? 0m })
            .FirstOrDefaultAsync(ct);

        var cancelledLast30 = await _db.bookings
            .CountAsync(b => b.status == "cancelled" && b.created_at >= last30, ct);

        var topTours = await _db.tours
            .Where(t => t.status == "active")
            .OrderByDescending(t => t.total_reviews)
            .ThenByDescending(t => t.avg_rating)
            .Take(5)
            .Select(t => new { t.name, t.avg_rating, t.total_reviews, t.price })
            .ToListAsync(ct);

        var docksLoad = await _db.docks
            .Select(d => new
            {
                d.name,
                d.max_boats,
                SchedulesLast30 = d.tour_schedules.Count(s => s.start_time >= last30)
            })
            .OrderByDescending(x => x.SchedulesLast30)
            .Take(10)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine($"Tính đến {now:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine($"- Users: {totalUsers}, Owners verified: {totalOwners}, Boats: {totalBoats}");
        sb.AppendLine($"- Tours: {totalActiveTours} active, {totalPendingTours} pending");
        sb.AppendLine($"- Owners chờ duyệt: {pendingOwners}");
        sb.AppendLine($"- Bookings tháng này: {bookingsThisMonth?.Count ?? 0}, doanh thu {bookingsThisMonth?.Revenue ?? 0:N0} VNĐ");
        sb.AppendLine($"- Bookings tháng trước: {bookingsLastMonth?.Count ?? 0}, doanh thu {bookingsLastMonth?.Revenue ?? 0:N0} VNĐ");
        sb.AppendLine($"- Bookings đã huỷ (30 ngày): {cancelledLast30}");
        sb.AppendLine("- Top 5 tour theo review:");
        foreach (var t in topTours)
            sb.AppendLine($"  • {t.name} — {t.avg_rating:F1}⭐ ({t.total_reviews} reviews) — {t.price:N0}đ");
        sb.AppendLine("- Dock utilization (30 ngày):");
        foreach (var d in docksLoad)
            sb.AppendLine($"  • {d.name} — {d.SchedulesLast30} tour / capacity {d.max_boats}");
        return sb.ToString();
    }

    public async IAsyncEnumerable<string> AskStreamAsync(
        Guid adminUserId,
        AdminOpsChatRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var question = (request.Question ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(question))
        {
            yield return "Vui lòng nhập câu hỏi.";
            yield break;
        }

        var toolResults = await ResolveToolsAsync(question, ct);
        var snapshot = await BuildSystemSnapshotAsync(ct);
        var prompt = $@"Bạn là **DDMS Admin Analyst**. Trả lời admin bằng tiếng Việt, dùng số liệu cụ thể. Súc tích, có bullet.

TOOL RESULTS (fetch động cho câu hỏi):
{toolResults}

SNAPSHOT CHUNG:
{snapshot}

CÂU HỎI: {question}";

        await foreach (var delta in StreamGeminiAsync(prompt, ct))
        {
            if (!string.IsNullOrEmpty(delta)) yield return delta;
        }
    }

    private async IAsyncEnumerable<string> StreamGeminiAsync(
        string prompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_gemini.ApiKey)) yield break;

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = _gemini.BuildGenerationConfig()
        };

        var models = _gemini.ModelCandidates;

        foreach (var model in models)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:streamGenerateContent?alt=sse&key={_gemini.ApiKey}";
            HttpResponseMessage? response = null;
            try
            {
                var httpReq = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
                };
                response = await _http.SendAsync(httpReq, HttpCompletionOption.ResponseHeadersRead, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Admin ops streaming failed on model {Model}", model);
                continue;
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                response?.Dispose();
                continue;
            }

            using (response)
            using (var stream = await response.Content.ReadAsStreamAsync(ct))
            using (var reader = new StreamReader(stream))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (ct.IsCancellationRequested) yield break;
                    if (!line.StartsWith("data:")) continue;
                    var payload = line.Substring(5).Trim();
                    if (string.IsNullOrEmpty(payload) || payload == "[DONE]") continue;
                    string? delta = null;
                    try
                    {
                        using var doc = JsonDocument.Parse(payload);
                        var candidates = doc.RootElement.GetProperty("candidates");
                        if (candidates.GetArrayLength() > 0)
                        {
                            var parts = candidates[0].GetProperty("content").GetProperty("parts");
                            if (parts.GetArrayLength() > 0)
                            {
                                delta = parts[0].GetProperty("text").GetString();
                            }
                        }
                    }
                    catch { }
                    if (!string.IsNullOrEmpty(delta)) yield return delta;
                }
            }
            yield break;
        }
    }

    // ─────────────────────────── What-if Simulator ───────────────────────────

    public async Task<WhatIfSimResponse> SimulateAsync(WhatIfSimRequest request, CancellationToken ct)
    {
        var start = request.StartDate ?? DateTime.UtcNow;
        var end = request.EndDate ?? start.AddDays(1);
        var scenario = (request.Scenario ?? string.Empty).ToLowerInvariant();

        return scenario switch
        {
            "close_dock" => await SimulateCloseDockAsync(request.DockId, start, end, ct),
            "bad_weather" => await SimulateBadWeatherAsync(start, end, ct),
            "add_boats" => await SimulateAddBoatsAsync(request.Number ?? 5, ct),
            _ => new WhatIfSimResponse
            {
                Scenario = scenario,
                Summary = "Scenario không hỗ trợ. Dùng: close_dock, bad_weather, add_boats.",
            },
        };
    }

    private async Task<WhatIfSimResponse> SimulateCloseDockAsync(Guid? dockId, DateTime start, DateTime end, CancellationToken ct)
    {
        if (dockId == null)
        {
            return new WhatIfSimResponse { Scenario = "close_dock", Summary = "Vui lòng chọn dock." };
        }

        var dock = await _db.docks.FirstOrDefaultAsync(d => d.id == dockId.Value, ct);
        if (dock == null)
        {
            return new WhatIfSimResponse { Scenario = "close_dock", Summary = "Không tìm thấy dock." };
        }

        var affectedSchedules = await _db.tour_schedules
            .Where(s => s.dock_id == dockId && s.start_time >= start && s.start_time < end)
            .ToListAsync(ct);
        var affectedScheduleIds = affectedSchedules.Select(s => s.id).ToList();

        var bookingsAgg = await _db.bookings
            .Where(b => affectedScheduleIds.Contains(b.schedule_id) && b.status != "cancelled")
            .GroupBy(b => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Guests = g.Sum(x => (int?)x.num_people) ?? 0,
                Revenue = g.Sum(x => (decimal?)x.total_price) ?? 0m,
            })
            .FirstOrDefaultAsync(ct);

        // Find nearby docks (any active dock other than this one) as candidates
        var otherDocks = await _db.docks
            .Where(d => d.id != dockId)
            .Select(d => new { d.name, d.max_boats })
            .Take(3)
            .ToListAsync(ct);

        var suggestions = new List<AlertItem>();
        if (otherDocks.Count > 0)
        {
            suggestions.Add(new AlertItem
            {
                Severity = "info",
                Title = "Chuyển sang dock khác",
                Detail = string.Join(", ", otherDocks.Select(d => $"{d.name} (cap {d.max_boats})")),
            });
        }
        suggestions.Add(new AlertItem
        {
            Severity = "info",
            Title = "Reschedule ngày khác",
            Detail = $"Nếu refund ~30% khách không thể đổi lịch: ước tính {(bookingsAgg?.Revenue ?? 0) * 0.3m:N0}đ",
        });

        return new WhatIfSimResponse
        {
            Scenario = "close_dock",
            Summary = $"Đóng {dock.name} từ {start:dd/MM HH:mm} đến {end:dd/MM HH:mm} sẽ ảnh hưởng "
                    + $"{affectedSchedules.Count} tour, {bookingsAgg?.Count ?? 0} booking, {bookingsAgg?.Guests ?? 0} khách.",
            AffectedBookings = bookingsAgg?.Count ?? 0,
            AffectedGuests = bookingsAgg?.Guests ?? 0,
            PotentialRefundVnd = (bookingsAgg?.Revenue ?? 0) * 0.3m,
            Suggestions = suggestions,
        };
    }

    private async Task<WhatIfSimResponse> SimulateBadWeatherAsync(DateTime start, DateTime end, CancellationToken ct)
    {
        var affected = await _db.tour_schedules
            .Where(s => s.start_time >= start && s.start_time < end)
            .Select(s => new
            {
                s.id,
                s.tour.name,
            })
            .ToListAsync(ct);

        var scheduleIds = affected.Select(a => a.id).ToList();
        var bookingsAgg = await _db.bookings
            .Where(b => scheduleIds.Contains(b.schedule_id) && b.status != "cancelled")
            .GroupBy(b => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Guests = g.Sum(x => (int?)x.num_people) ?? 0,
                Revenue = g.Sum(x => (decimal?)x.total_price) ?? 0m,
            })
            .FirstOrDefaultAsync(ct);

        // Assume 40% cancel-rate for outdoor tours under bad weather
        var estCancel = (int)Math.Round((bookingsAgg?.Count ?? 0) * 0.4);
        var estRefund = (bookingsAgg?.Revenue ?? 0) * 0.4m;

        return new WhatIfSimResponse
        {
            Scenario = "bad_weather",
            Summary = $"Thời tiết xấu {start:dd/MM}-{end:dd/MM}: {affected.Count} tour có nguy cơ ảnh hưởng, "
                    + $"ước tính ~{estCancel}/{bookingsAgg?.Count ?? 0} booking sẽ huỷ.",
            AffectedBookings = estCancel,
            AffectedGuests = (int)Math.Round((bookingsAgg?.Guests ?? 0) * 0.4),
            PotentialRefundVnd = estRefund,
            Suggestions = new List<AlertItem>
            {
                new() {
                    Severity = "warning",
                    Title = "Chủ động thông báo",
                    Detail = "Gửi email + push cho khách 24h trước để dời lịch → giảm refund",
                },
                new() {
                    Severity = "info",
                    Title = "Ưu tiên tour indoor/có mái",
                    Detail = "Đẩy marketing cho tour không phụ thuộc thời tiết trong ngày mưa",
                },
            },
        };
    }

    private async Task<WhatIfSimResponse> SimulateAddBoatsAsync(int number, CancellationToken ct)
    {
        var totalDocks = await _db.docks.CountAsync(ct);
        var totalCapacity = await _db.docks.SumAsync(d => (int?)d.max_boats, ct) ?? 0;
        var totalActiveBoats = await _db.boats.CountAsync(b => b.status != "retired", ct);

        var afterCapacityUsage = totalCapacity > 0
            ? (int)Math.Round(100.0 * (totalActiveBoats + number) / totalCapacity)
            : 0;

        var suggestion = afterCapacityUsage > 90
            ? "Cần đầu tư thêm dock — utilization sẽ vượt 90%."
            : afterCapacityUsage > 70
                ? "Ok, capacity còn cover được nhưng cần theo dõi giờ cao điểm."
                : "Dư sức chứa — có thể mở rộng thêm boats.";

        return new WhatIfSimResponse
        {
            Scenario = "add_boats",
            Summary = $"Thêm {number} boat mới: total {totalActiveBoats + number}/{totalCapacity} slot "
                    + $"(utilization {afterCapacityUsage}%). {suggestion}",
            AffectedBookings = 0,
            AffectedGuests = 0,
            Suggestions = new List<AlertItem>
            {
                new() { Severity = "info", Title = "Ước lượng doanh thu tăng", Detail = $"~{number * 40}tr/tháng (giả định 40tr/boat)" },
            },
        };
    }

    // ─────────────────────────── Gemini helper ───────────────────────────

    private async Task<string?> CallGeminiAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_gemini.ApiKey)) return null;

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = _gemini.BuildGenerationConfig()
        };

        var models = _gemini.ModelCandidates;

        foreach (var model in models)
        {
            try
            {
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_gemini.ApiKey}";
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var res = await _http.PostAsync(endpoint, content);
                if (!res.IsSuccessStatusCode) continue;
                var body = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var candidates = doc.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    if (!string.IsNullOrWhiteSpace(text)) return text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AdminOps Gemini failed on model {Model}", model);
            }
        }
        return null;
    }
}
