using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DDMS.Backend.Configurations;
using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Ai;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

public class GeminiAiService : IAiService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(
        AppDbContext context,
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        ILogger<GeminiAiService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiChatResponseDto> SendMessageAsync(Guid userId, AiChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message cannot be empty.");
        }

        // 1. Find or create AI Conversation
        ai_conversation conversation;
        if (request.ConversationId.HasValue && request.ConversationId.Value != Guid.Empty)
        {
            conversation = await _context.ai_conversations
                .FirstOrDefaultAsync(c => c.id == request.ConversationId.Value && c.user_id == userId)
                ?? await CreateNewConversationAsync(userId, request.Message);
        }
        else
        {
            conversation = await CreateNewConversationAsync(userId, request.Message);
        }

        // 2. Save User Message
        var userMsg = new ai_message
        {
            id = Guid.NewGuid(),
            ai_conversation_id = conversation.id,
            role = "user",
            content = request.Message,
            created_at = DateTime.UtcNow
        };
        _context.ai_messages.Add(userMsg);
        conversation.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 3. Build RAG Context from Database
        var ragContext = await BuildRagContextAsync();

        // 4. Retrieve recent message history (last 10 messages)
        var recentHistory = await _context.ai_messages
            .Where(m => m.ai_conversation_id == conversation.id)
            .OrderByDescending(m => m.created_at)
            .Take(10)
            .OrderBy(m => m.created_at)
            .ToListAsync();

        // 5. Call Gemini API or fallback
        string aiReplyText;
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            aiReplyText = await CallGeminiApiAsync(ragContext, recentHistory, request.Message);
        }
        else
        {
            aiReplyText = GenerateFallbackResponse(request.Message, ragContext.Tours);
        }

        // 6. Save AI Response
        var aiMsg = new ai_message
        {
            id = Guid.NewGuid(),
            ai_conversation_id = conversation.id,
            role = "assistant",
            content = aiReplyText,
            created_at = DateTime.UtcNow
        };
        _context.ai_messages.Add(aiMsg);
        await _context.SaveChangesAsync();

        // 7. Extract recommended tour IDs if mentioned or relevant
        var recommendedTours = ExtractRecommendedTours(aiReplyText, ragContext.Tours, request.Message);

        return new AiChatResponseDto
        {
            ConversationId = conversation.id,
            Reply = aiReplyText,
            RecommendedTours = recommendedTours
        };
    }

    public async Task<List<AiConversationDto>> GetUserConversationsAsync(Guid userId)
    {
        return await _context.ai_conversations
            .Where(c => c.user_id == userId)
            .OrderByDescending(c => c.updated_at)
            .Select(c => new AiConversationDto
            {
                Id = c.id,
                Title = c.title ?? "Hội thoại tư vấn AI",
                CreatedAt = c.created_at,
                UpdatedAt = c.updated_at,
                MessageCount = c.ai_messages.Count
            })
            .ToListAsync();
    }

    public async Task<List<AiMessageDto>> GetConversationMessagesAsync(Guid userId, Guid conversationId)
    {
        var conversationExists = await _context.ai_conversations
            .AnyAsync(c => c.id == conversationId && c.user_id == userId);

        if (!conversationExists)
        {
            return new List<AiMessageDto>();
        }

        return await _context.ai_messages
            .Where(m => m.ai_conversation_id == conversationId)
            .OrderBy(m => m.created_at)
            .Select(m => new AiMessageDto
            {
                Id = m.id,
                SenderRole = m.role,
                Content = m.content,
                CreatedAt = m.created_at
            })
            .ToListAsync();
    }

    public async Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId)
    {
        var conversation = await _context.ai_conversations
            .FirstOrDefaultAsync(c => c.id == conversationId && c.user_id == userId);

        if (conversation == null) return false;

        var messages = await _context.ai_messages
            .Where(m => m.ai_conversation_id == conversationId)
            .ToListAsync();

        _context.ai_messages.RemoveRange(messages);
        _context.ai_conversations.Remove(conversation);
        await _context.SaveChangesAsync();
        return true;
    }

    #region Streaming

    public async IAsyncEnumerable<AiStreamChunk> SendMessageStreamAsync(
        Guid userId,
        AiChatRequestDto request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            yield return new AiStreamChunk { Type = "error", Error = "Message cannot be empty." };
            yield break;
        }

        ai_conversation conversation;
        if (request.ConversationId.HasValue && request.ConversationId.Value != Guid.Empty)
        {
            conversation = await _context.ai_conversations
                .FirstOrDefaultAsync(c => c.id == request.ConversationId.Value && c.user_id == userId, ct)
                ?? await CreateNewConversationAsync(userId, request.Message);
        }
        else
        {
            conversation = await CreateNewConversationAsync(userId, request.Message);
        }

        var userMsg = new ai_message
        {
            id = Guid.NewGuid(),
            ai_conversation_id = conversation.id,
            role = "user",
            content = request.Message,
            created_at = DateTime.UtcNow
        };
        _context.ai_messages.Add(userMsg);
        conversation.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var ragContext = await BuildRagContextAsync();
        var recentHistory = await _context.ai_messages
            .Where(m => m.ai_conversation_id == conversation.id)
            .OrderByDescending(m => m.created_at)
            .Take(10)
            .OrderBy(m => m.created_at)
            .ToListAsync(ct);

        var fullText = new StringBuilder();
        await foreach (var delta in StreamGeminiApiAsync(ragContext, recentHistory, request.Message, ct))
        {
            fullText.Append(delta);
            yield return new AiStreamChunk
            {
                Type = "chunk",
                Delta = delta,
                ConversationId = conversation.id,
            };
        }

        var finalText = fullText.ToString();
        if (string.IsNullOrWhiteSpace(finalText))
        {
            finalText = GenerateFallbackResponse(request.Message, ragContext.Tours);
            yield return new AiStreamChunk
            {
                Type = "chunk",
                Delta = finalText,
                ConversationId = conversation.id,
            };
        }

        var aiMsg = new ai_message
        {
            id = Guid.NewGuid(),
            ai_conversation_id = conversation.id,
            role = "assistant",
            content = finalText,
            created_at = DateTime.UtcNow
        };
        _context.ai_messages.Add(aiMsg);
        await _context.SaveChangesAsync(ct);

        var tours = ExtractRecommendedTours(finalText, ragContext.Tours, request.Message);
        yield return new AiStreamChunk
        {
            Type = "done",
            ConversationId = conversation.id,
            RecommendedTours = tours,
        };
    }

    private async IAsyncEnumerable<string> StreamGeminiApiAsync(
        RagContextData ragContext,
        List<ai_message> history,
        string currentPrompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            yield break;
        }

        // Build the same system prompt + contents as non-streaming (extracted helper)
        var (contentsList, generationConfig) = BuildGeminiRequest(ragContext, history, currentPrompt);

        var requestBody = new
        {
            contents = contentsList,
            generationConfig = generationConfig,
        };

        var modelsToTry = _options.ModelCandidates;

        foreach (var modelName in modelsToTry)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:streamGenerateContent?alt=sse&key={_options.ApiKey}";
            HttpResponseMessage? response = null;
            try
            {
                var httpReq = new HttpRequestMessage(HttpMethod.Post, endpoint);
                httpReq.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");
                response = await _httpClient.SendAsync(httpReq, HttpCompletionOption.ResponseHeadersRead, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini streaming failed on model {Model}", modelName);
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
                    if (payload == "[DONE]") yield break;
                    if (string.IsNullOrWhiteSpace(payload)) continue;

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
                    catch
                    {
                        // Ignore malformed lines
                    }
                    if (!string.IsNullOrEmpty(delta)) yield return delta;
                }
            }
            yield break; // succeeded on this model
        }
    }

    private (List<object> contents, object generationConfig) BuildGeminiRequest(
        RagContextData ragContext,
        List<ai_message> history,
        string currentPrompt)
    {
        var systemInstruction = new StringBuilder();
        systemInstruction.AppendLine("Bạn là **DDMS Trip Concierge** — chuyên viên tư vấn du lịch đường thủy tại Đà Nẵng.");
        systemInstruction.AppendLine("Ngắn gọn, chuyên nghiệp, đưa `[ID: {guid}]` sau mỗi tên tour đề xuất.");
        systemInstruction.AppendLine("--- TOUR ACTIVE ---");
        foreach (var t in ragContext.Tours.Take(20))
        {
            var rating = t.TotalReviews > 0 ? $"⭐{t.AvgRating:F1}" : "";
            systemInstruction.AppendLine($"- [ID: {t.Id}] {t.Title} | {t.Price:N0}đ | {t.Duration} | {rating}");
        }
        if (ragContext.Weather.Count > 0)
        {
            systemInstruction.AppendLine("--- WEATHER 7d ---");
            foreach (var w in ragContext.Weather)
                systemInstruction.AppendLine($"- {w.Date}: {w.Summary}, {w.MinTempC:F0}-{w.MaxTempC:F0}°C");
        }

        var contentsList = new List<object>
        {
            new { role = "user", parts = new[] { new { text = systemInstruction.ToString() } } },
            new { role = "model", parts = new[] { new { text = "OK, tôi sẵn sàng." } } },
        };
        foreach (var msg in history.Take(history.Count - 1))
        {
            contentsList.Add(new
            {
                role = msg.role == "user" ? "user" : "model",
                parts = new[] { new { text = msg.content } }
            });
        }
        contentsList.Add(new { role = "user", parts = new[] { new { text = currentPrompt } } });

        var generationConfig = _options.BuildGenerationConfig();
        return (contentsList, generationConfig);
    }

    #endregion

    #region Owner Content Studio

    public async Task<OwnerContentResponseDto> GenerateOwnerContentAsync(OwnerContentRequestDto request)
    {
        var type = (request.Type ?? string.Empty).ToLowerInvariant();
        var keywords = (request.Keywords ?? string.Empty).Trim();

        var prompt = type switch
        {
            "name" => BuildNamePrompt(keywords, request.ServiceType, request.TourName),
            "description" => BuildDescriptionPrompt(keywords, request.ServiceType, request.TourName, request.DurationMinutes),
            "faqs" => BuildFaqPrompt(request.TourName, request.Description, keywords),
            "price" => await BuildPricePromptAsync(keywords, request.ServiceType, request.DurationMinutes),
            _ => throw new ArgumentException($"Unsupported content type: {request.Type}"),
        };

        var raw = await CallGeminiForContentAsync(prompt);
        _logger.LogInformation("[ContentStudio type={Type}] raw Gemini response (first 500 chars): {Preview}",
            type, raw?.Length > 500 ? raw.Substring(0, 500) : raw);

        return type switch
        {
            "name" => new OwnerContentResponseDto
            {
                Type = "name",
                Options = ParseNameOptions(raw),
                Text = raw?.Trim(), // include raw so FE can show fallback
            },
            "description" => new OwnerContentResponseDto
            {
                Type = "description",
                Text = raw.Trim(),
            },
            "faqs" => new OwnerContentResponseDto
            {
                Type = "faqs",
                Faqs = ParseFaqs(raw),
            },
            "price" => new OwnerContentResponseDto
            {
                Type = "price",
                Text = raw.Trim(),
                SuggestedPrice = ExtractPrice(raw),
            },
            _ => new OwnerContentResponseDto { Type = type, Text = raw }
        };
    }

    private static string BuildNamePrompt(string keywords, string? serviceType, string? tourName)
    {
        // Owner thường gõ ý tưởng thẳng vào ô tên tour — dùng làm ngữ cảnh khi keywords trống.
        var kw = string.IsNullOrWhiteSpace(keywords)
            ? (string.IsNullOrWhiteSpace(tourName) ? "(chưa có)" : tourName)
            : keywords;
        return $@"Bạn là copywriter marketing tour du lịch Đà Nẵng.
Nhiệm vụ: Gợi ý **5 tên tour** hấp dẫn, độ dài 5-10 từ mỗi tên.
- Nếu từ khoá vắn tắt/mơ hồ, hãy sáng tạo dựa trên đặc trưng Đà Nẵng (sông Hàn, cầu Rồng, hoàng hôn, Cù Lao Chàm, Sơn Trà, hải sản, du thuyền, cano...).
- KHÔNG bao giờ trả về danh sách rỗng.

Loại dịch vụ: {serviceType ?? "tour"}
Từ khoá: {kw}

FORMAT OUTPUT BẮT BUỘC — đúng 5 dòng, mỗi dòng 1 tên, đánh số 1-5, KHÔNG bold, KHÔNG giải thích:
1. Tên tour thứ nhất
2. Tên tour thứ hai
3. Tên tour thứ ba
4. Tên tour thứ tư
5. Tên tour thứ năm";
    }

    private static string BuildDescriptionPrompt(string keywords, string? serviceType, string? tourName, int? durationMinutes)
    {
        var nameLine = string.IsNullOrWhiteSpace(tourName) ? "" : $"Tên tour: {tourName}\n";
        var durationLine = durationMinutes.HasValue ? $"Thời lượng: {durationMinutes} phút\n" : "";
        return $@"Bạn là copywriter du lịch. Viết mô tả tour du thuyền tại Đà Nẵng, giọng văn:
- Chuyên nghiệp, cuốn hút, gợi cảm xúc.
- KHÔNG dùng emoji.
- Nhấn mạnh trải nghiệm (không phải liệt kê feature).
- Độ dài 150-220 từ.

{nameLine}Loại dịch vụ: {serviceType ?? "tour"}
{durationLine}Từ khoá: {keywords}

Chỉ output đoạn văn mô tả, KHÔNG có tiêu đề hay bullet.";
    }

    private static string BuildFaqPrompt(string? tourName, string? description, string keywords)
    {
        var context = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(tourName)) context.AppendLine($"Tên tour: {tourName}");
        if (!string.IsNullOrWhiteSpace(description)) context.AppendLine($"Mô tả: {description}");
        if (!string.IsNullOrWhiteSpace(keywords)) context.AppendLine($"Từ khoá: {keywords}");

        return $@"Bạn là chuyên viên chăm sóc khách hàng du lịch.
Từ thông tin tour bên dưới, sinh **6-8 câu FAQ** mà khách hàng thường hỏi trước khi đặt.
Mỗi FAQ gồm câu hỏi ngắn gọn + câu trả lời rõ ràng (2-3 câu).

Thông tin tour:
{context}

Output định dạng JSON, KHÔNG giải thích thêm, KHÔNG markdown code block:
[
  {{""question"":""..."",""answer"":""...""}},
  ...
]";
    }

    private async Task<string> BuildPricePromptAsync(string keywords, string? serviceType, int? durationMinutes)
    {
        // Fetch similar tours from DB for context
        var similarTours = await _context.tours
            .Where(t => t.status == "active")
            .OrderByDescending(t => t.total_reviews)
            .Take(15)
            .Select(t => new { t.name, t.price, t.duration_minutes })
            .ToListAsync();

        var similarList = string.Join("\n", similarTours.Select(t =>
            $"- {t.name}: {t.price:N0}đ ({t.duration_minutes} phút)"));

        return $@"Bạn là chuyên gia định giá tour du lịch.
Dựa trên dữ liệu tour tương tự trên platform Đà Nẵng bên dưới, gợi ý **giá vé cơ bản** (VNĐ) cho tour mới với thông tin sau:
- Loại: {serviceType ?? "tour"}
- Thời lượng: {(durationMinutes.HasValue ? durationMinutes + " phút" : "chưa xác định")}
- Từ khoá: {keywords}

Tour tương tự:
{similarList}

Output định dạng chính xác (KHÔNG markdown):
GIÁ: 350000
LÝ DO: (1-2 câu giải thích ngắn gọn tại sao giá này hợp lý)";
    }

    private async Task<string> CallGeminiForContentAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return "AI chưa được cấu hình. Vui lòng thử lại sau.";
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = _options.BuildGenerationConfig()
        };

        var modelsToTry = _options.ModelCandidates;

        foreach (var modelName in modelsToTry)
        {
            try
            {
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_options.ApiKey}";
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Gemini content-gen ({Model}) returned {StatusCode}: {Body}",
                        modelName, response.StatusCode, errBody);
                    continue;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var candidates = doc.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                    if (!string.IsNullOrWhiteSpace(text)) return text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini content-gen call failed on model {Model}", modelName);
            }
        }
        return string.Empty;
    }

    private static List<string> ParseNameOptions(string raw)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(raw)) return list;

        var lines = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Skip lines that are pure meta text (headings, intros)
            if (Regex.IsMatch(trimmed, @"^(Dưới đây|Sau đây|Xin gợi ý|Gợi ý|Chào|Đây là|Note:|Ghi chú)", RegexOptions.IgnoreCase))
                continue;

            // Strip leading list markers: "1.", "1)", "1 -", "- ", "* ", "• "
            var match = Regex.Match(trimmed, @"^(?:\d+[\.\)\-\s]|[\-\*•▪●○▶►]\s)+(.+)$");
            var name = match.Success ? match.Groups[1].Value.Trim() : trimmed;

            // Strip markdown bold/italic wrappers
            name = Regex.Replace(name, @"^\**([^*].*?)\**$", "$1").Trim();
            name = name.Trim().TrimEnd('.', '。', ',', ';');

            // Filter out very short or very long entries
            if (name.Length >= 3 && name.Length <= 120)
            {
                list.Add(name);
            }
        }
        return list.Take(5).ToList();
    }

    private static List<FaqItem> ParseFaqs(string raw)
    {
        var jsonStart = raw.IndexOf('[');
        var jsonEnd = raw.LastIndexOf(']');
        if (jsonStart < 0 || jsonEnd <= jsonStart) return new List<FaqItem>();
        var jsonPart = raw.Substring(jsonStart, jsonEnd - jsonStart + 1);
        try
        {
            var parsed = JsonSerializer.Deserialize<List<FaqItem>>(jsonPart, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            return parsed ?? new List<FaqItem>();
        }
        catch
        {
            return new List<FaqItem>();
        }
    }

    private static decimal? ExtractPrice(string raw)
    {
        var match = Regex.Match(raw, @"GI[ÁA]\s*[:：]\s*([\d,\.]+)", RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        var digits = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return decimal.TryParse(digits, out var p) ? p : null;
    }

    #endregion

    #region Helper Methods

    private async Task<ai_conversation> CreateNewConversationAsync(Guid userId, string initialMessage)
    {
        string title = initialMessage.Length > 40
            ? initialMessage.Substring(0, 40) + "..."
            : initialMessage;

        var conv = new ai_conversation
        {
            id = Guid.NewGuid(),
            user_id = userId,
            title = title,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        _context.ai_conversations.Add(conv);
        await _context.SaveChangesAsync();
        return conv;
    }

    private class RagContextData
    {
        public List<TourRagInfo> Tours { get; set; } = new();
        public List<string> Docks { get; set; } = new();
        public List<WeatherDay> Weather { get; set; } = new();
    }

    private class WeatherDay
    {
        public string Date { get; set; } = string.Empty;
        public double MaxTempC { get; set; }
        public double MinTempC { get; set; }
        public int WeatherCode { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    private class TourRagInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string? Duration { get; set; }
        public string? ImageUrl { get; set; }
        public string? BoatName { get; set; }
        public string? DepartureDock { get; set; }
        public string? Description { get; set; }
        public decimal AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public string? CancelPolicy { get; set; }
        public string Status { get; set; } = "active";
        public int UpcomingSchedules { get; set; }
        public DateTime? NextDeparture { get; set; }
    }

    private async Task<RagContextData> BuildRagContextAsync()
    {
        var now = DateTime.UtcNow;
        var tours = await _context.tours
            .Where(t => t.status == "active")
            .Include(t => t.tour_schedules)
                .ThenInclude(s => s.boat)
            .Include(t => t.tour_images)
            .OrderByDescending(t => t.avg_rating)
            .ThenByDescending(t => t.total_reviews)
            .Take(40)
            .Select(t => new TourRagInfo
            {
                Id = t.id,
                Title = t.name,
                Price = t.price,
                DurationMinutes = t.duration_minutes,
                Duration = t.duration_minutes >= 60
                    ? $"{t.duration_minutes / 60}h{(t.duration_minutes % 60 > 0 ? $"{t.duration_minutes % 60}m" : "")}"
                    : $"{t.duration_minutes} phút",
                ImageUrl = t.tour_images
                    .OrderBy(i => i.sort_order)
                    .Select(i => i.image_url)
                    .FirstOrDefault(),
                BoatName = t.tour_schedules
                    .Where(s => s.boat != null)
                    .Select(s => s.boat!.name)
                    .FirstOrDefault() ?? "Tàu Du Thuyền Sông Hàn",
                DepartureDock = t.location ?? "Bến du thuyền Sông Hàn",
                Description = t.description,
                AvgRating = t.avg_rating,
                TotalReviews = t.total_reviews,
                CancelPolicy = t.cancel_policy,
                Status = t.status,
                UpcomingSchedules = t.tour_schedules.Count(s => s.start_time >= now),
                NextDeparture = t.tour_schedules
                    .Where(s => s.start_time >= now)
                    .OrderBy(s => s.start_time)
                    .Select(s => (DateTime?)s.start_time)
                    .FirstOrDefault(),
            })
            .ToListAsync();

        var docks = await _context.docks
            .Select(d => d.name)
            .Take(10)
            .ToListAsync();

        var weather = await FetchWeatherForecastAsync();

        return new RagContextData
        {
            Tours = tours,
            Docks = docks,
            Weather = weather,
        };
    }

    private async Task<List<WeatherDay>> FetchWeatherForecastAsync()
    {
        try
        {
            // Da Nang coordinates
            var url = "https://api.open-meteo.com/v1/forecast?latitude=16.0544&longitude=108.2022"
                    + "&daily=weather_code,temperature_2m_max,temperature_2m_min"
                    + "&timezone=Asia%2FHo_Chi_Minh&forecast_days=7";
            var res = await _httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new List<WeatherDay>();
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var daily = doc.RootElement.GetProperty("daily");
            var dates = daily.GetProperty("time");
            var codes = daily.GetProperty("weather_code");
            var maxTemps = daily.GetProperty("temperature_2m_max");
            var minTemps = daily.GetProperty("temperature_2m_min");
            var result = new List<WeatherDay>();
            for (int i = 0; i < dates.GetArrayLength(); i++)
            {
                var code = codes[i].GetInt32();
                result.Add(new WeatherDay
                {
                    Date = dates[i].GetString() ?? "",
                    MaxTempC = maxTemps[i].GetDouble(),
                    MinTempC = minTemps[i].GetDouble(),
                    WeatherCode = code,
                    Summary = DescribeWeatherCode(code),
                });
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch weather forecast");
            return new List<WeatherDay>();
        }
    }

    private static string DescribeWeatherCode(int code)
    {
        if (code == 0) return "Trời quang";
        if (code >= 1 && code <= 3) return "Có mây";
        if (code == 45 || code == 48) return "Sương mù";
        if (code >= 51 && code <= 55) return "Mưa phùn";
        if (code >= 61 && code <= 65) return "Mưa";
        if (code >= 71 && code <= 77) return "Tuyết";
        if (code >= 80 && code <= 82) return "Mưa rào";
        if (code >= 95 && code <= 99) return "Dông";
        return "Không rõ";
    }

    private async Task<string> CallGeminiApiAsync(RagContextData ragContext, List<ai_message> history, string currentPrompt)
    {
        try
        {
            var systemInstruction = new StringBuilder();
            systemInstruction.AppendLine("Bạn là **DDMS Trip Concierge** — chuyên viên tư vấn du lịch đường thủy tại Đà Nẵng.");
            systemInstruction.AppendLine("Vai trò: giúp khách xây dựng hành trình phù hợp nhất (1 tour đơn hoặc combo nhiều tour cho chuyến đi nhiều ngày).");
            systemInstruction.AppendLine();
            systemInstruction.AppendLine("PHONG CÁCH GIAO TIẾP:");
            systemInstruction.AppendLine("- Thân thiện, chuyên nghiệp, chủ động — như concierge khách sạn 5 sao.");
            systemInstruction.AppendLine("- Ngắn gọn, không dài dòng. Dùng emoji vừa phải (🚢 🌅 ⭐).");
            systemInstruction.AppendLine("- Trả lời bằng ngôn ngữ khách dùng (mặc định tiếng Việt).");
            systemInstruction.AppendLine();
            systemInstruction.AppendLine("QUY TRÌNH TƯ VẤN:");
            systemInstruction.AppendLine("1. Nếu khách hỏi chung chung, hỏi lại 1-2 câu ngắn để làm rõ: **số người, ngày đi, budget, sở thích** (gia đình/lãng mạn/phượt/foodie).");
            systemInstruction.AppendLine("2. Nếu khách đã cung cấp đủ context, đề xuất tour luôn — không hỏi thêm.");
            systemInstruction.AppendLine("3. Cho trip nhiều ngày: đề xuất combo 2-3 tour, ghi rõ ngày nào tour nào và tổng chi phí.");
            systemInstruction.AppendLine("4. Với mỗi tour đề xuất, giải thích ngắn **VÌ SAO phù hợp** (1 câu).");
            systemInstruction.AppendLine("5. Kết thúc bằng câu hỏi mở để tiếp tục (đổi lịch, thêm option, đặt luôn?).");
            systemInstruction.AppendLine();
            systemInstruction.AppendLine("QUY TẮC HIỂN THỊ:");
            systemInstruction.AppendLine("- **BẮT BUỘC** chèn mã tour dạng `[ID: {guid}]` ngay sau tên tour đề xuất — để hệ thống render card. Ví dụ: '**Tour Ngắm Hoàng Hôn** [ID: 8e21ed58-f0f8-46a3-ba0b-3a1aab67e133]'.");
            systemInstruction.AppendLine("- Chỉ đề xuất tour có `status=active` và có `UpcomingSchedules > 0`.");
            systemInstruction.AppendLine("- Nếu không có tour phù hợp, nói thật + gợi ý gần nhất.");
            systemInstruction.AppendLine();
            systemInstruction.AppendLine("--- DANH SÁCH TOUR THỰC TẾ (đã sắp xếp theo rating giảm dần) ---");

            foreach (var t in ragContext.Tours)
            {
                var rating = t.TotalReviews > 0
                    ? $"⭐{t.AvgRating:F1}({t.TotalReviews})"
                    : "chưa có review";
                var nextDep = t.NextDeparture.HasValue
                    ? t.NextDeparture.Value.ToLocalTime().ToString("dd/MM HH:mm")
                    : "chưa mở lịch";
                systemInstruction.AppendLine($"- [ID: {t.Id}] | {t.Title} | {t.Price:N0}đ | {t.Duration} | {t.BoatName} | Bến: {t.DepartureDock} | {rating} | Lịch gần nhất: {nextDep} | Huỷ: {t.CancelPolicy}");
                if (!string.IsNullOrWhiteSpace(t.Description))
                {
                    var shortDesc = t.Description.Length > 120 ? t.Description.Substring(0, 120) + "..." : t.Description;
                    systemInstruction.AppendLine($"  → {shortDesc}");
                }
            }
            systemInstruction.AppendLine("--------------------------------");

            if (ragContext.Weather.Count > 0)
            {
                systemInstruction.AppendLine();
                systemInstruction.AppendLine("--- THỜI TIẾT ĐÀ NẴNG 7 NGÀY TỚI (open-meteo) ---");
                foreach (var w in ragContext.Weather)
                {
                    systemInstruction.AppendLine($"- {w.Date}: {w.Summary}, {w.MinTempC:F0}-{w.MaxTempC:F0}°C");
                }
                systemInstruction.AppendLine("Khi khách hỏi tour outdoor/lặn biển, HÃY CẢNH BÁO nếu ngày đó có mưa/dông. Gợi ý ngày đẹp thay thế.");
            }

            systemInstruction.AppendLine();
            systemInstruction.AppendLine("VÍ DỤ RESPONSE TỐT:");
            systemInstruction.AppendLine("Khách: 'Tôi có 2 ngày cuối tuần cùng vợ ở Đà Nẵng, budget 2 triệu, thích lãng mạn.'");
            systemInstruction.AppendLine("Bạn: 'Tuyệt! Mình gợi ý combo 2 ngày lãng mạn:");
            systemInstruction.AppendLine("📅 **Thứ 7 chiều** — Tour Ngắm Hoàng Hôn Sông Hàn [ID: 8e21...] (450k/người) — thuyền có bàn riêng, view Cầu Rồng phun lửa.");
            systemInstruction.AppendLine("📅 **Chủ nhật sáng** — Du thuyền Sông Hàn + trà chiều [ID: 9f12...] (550k/người) — không gian riêng tư, tráng miệng cao cấp.");
            systemInstruction.AppendLine("Tổng: 2 triệu cho 2 người, vẫn dư ~100k cho tip guide 💐. Muốn mình check lịch trống ngay không?'");

            var contentsList = new List<object>();

            // Add system instruction as first user message or system instruction
            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = systemInstruction.ToString() } }
            });
            contentsList.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Xin chào! Tôi là Trợ lý AI du thuyền DDMS. Tôi đã sẵn sàng hỗ trợ tư vấn tour cho bạn!" } }
            });

            // Add message history
            foreach (var msg in history.Take(history.Count - 1))
            {
                contentsList.Add(new
                {
                    role = msg.role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.content } }
                });
            }

            // Current prompt
            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = currentPrompt } }
            });

            var requestBody = new
            {
                contents = contentsList,
                generationConfig = _options.BuildGenerationConfig()
            };

            var modelsToTry = _options.ModelCandidates;

            foreach (var modelName in modelsToTry)
            {
                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_options.ApiKey}";
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    var candidates = doc.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            return text;
                        }
                    }
                }
                else
                {
                    var errStr = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Gemini API ({Model}) returned error: {StatusCode} - {Body}", modelName, response.StatusCode, errStr);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking Gemini API");
        }

        return GenerateFallbackResponse(currentPrompt, ragContext.Tours);
    }

    private string GenerateFallbackResponse(string userPrompt, List<TourRagInfo> tours)
    {
        var lowerPrompt = userPrompt.Trim().ToLower();

        // Handle simple greetings
        var isGreeting = lowerPrompt is "hello" or "hi" or "chào" or "xin chào" or "chào bạn" or "hế lô" or "helo" or "cảm ơn" or "thanks" or "thank you";
        if (isGreeting)
        {
            return "Xin chào! 🚢 Tôi là Trợ lý AI du thuyền DDMS. Tôi có thể giúp bạn tìm kiếm tour du thuyền sông Hàn, tư vấn giá vé, chọn du thuyền hoặc kiểm tra lịch khởi hành. Bạn đang tìm tour du thuyền nào hôm nay ạ?";
        }

        if (tours.Count == 0)
        {
            return "Xin chào! Trợ lý AI du thuyền DDMS xin chào bạn. Hiện tại hệ thống đang cập nhật danh sách tour du thuyền sông Hàn Đà Nẵng. Bạn vui lòng quay lại sau ít phút!";
        }

        var matchingTour = tours.FirstOrDefault(t =>
            lowerPrompt.Contains("cầu rồng") || lowerPrompt.Contains("sông hàn") || lowerPrompt.Contains("ăn tối") || lowerPrompt.Contains("tối"))
            ?? tours.First();

        var sb = new StringBuilder();
        sb.AppendLine($"Dựa trên câu hỏi của bạn, Trợ lý AI DDMS xin gợi ý tour du thuyền phù hợp nhất:");
        sb.AppendLine($"🚢 **{matchingTour.Title}**");
        sb.AppendLine($"• **Giá vé:** {matchingTour.Price:N0} VNĐ / khách");
        sb.AppendLine($"• **Thời lượng:** {matchingTour.Duration ?? "90 phút"}");
        sb.AppendLine($"• **Bến xuất phát:** {matchingTour.DepartureDock ?? "Bến du thuyền Sông Hàn"}");
        sb.AppendLine($"• **Mô tả:** Tour trải nghiệm ngắm cảnh đêm lung linh trên Sông Hàn, xem Cầu Rồng phun lửa & nước cực kỳ hấp dẫn!");
        sb.AppendLine("\nBạn có muốn tôi hỗ trợ kiểm tra lịch khởi hành hoặc giữ chỗ trực tuyến ngay bây giờ không?");

        return sb.ToString();
    }

    private List<AiRecommendedTourDto> ExtractRecommendedTours(string aiText, List<TourRagInfo> tours, string userPrompt)
    {
        var result = new List<AiRecommendedTourDto>();
        if (tours.Count == 0) return result;

        var lowerPrompt = userPrompt.Trim().ToLower();
        var isGreeting = lowerPrompt is "hello" or "hi" or "chào" or "xin chào" or "chào bạn" or "hế lô" or "helo" or "cảm ơn" or "thanks" or "thank you";

        // Match GUID IDs in AI response
        var guidRegex = new Regex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        var matches = guidRegex.Matches(aiText);

        foreach (Match match in matches)
        {
            if (Guid.TryParse(match.Value, out Guid id))
            {
                var tour = tours.FirstOrDefault(t => t.Id == id);
                if (tour != null && !result.Any(r => r.Id == tour.Id))
                {
                    result.Add(new AiRecommendedTourDto
                    {
                        Id = tour.Id,
                        Title = tour.Title,
                        Price = tour.Price,
                        Duration = tour.Duration,
                        ImageUrl = tour.ImageUrl,
                        DepartureLocation = tour.DepartureDock,
                        AvgRating = tour.AvgRating,
                        TotalReviews = tour.TotalReviews,
                        NextDeparture = tour.NextDeparture,
                    });
                }
            }
        }

        // If no GUID matched and user was just saying hello/greeting, return no cards
        if (result.Count == 0 && isGreeting)
        {
            return result;
        }

        // Fallback: If no explicit GUID matched, match keywords
        if (result.Count == 0)
        {
            var lower = (aiText + " " + userPrompt).ToLower();
            var matchesByTitle = tours.Where(t => lower.Contains(t.Title.ToLower())).Take(2).ToList();
            if (matchesByTitle.Count > 0)
            {
                foreach (var tour in matchesByTitle)
                {
                    result.Add(new AiRecommendedTourDto
                    {
                        Id = tour.Id,
                        Title = tour.Title,
                        Price = tour.Price,
                        Duration = tour.Duration,
                        ImageUrl = tour.ImageUrl,
                        DepartureLocation = tour.DepartureDock,
                        AvgRating = tour.AvgRating,
                        TotalReviews = tour.TotalReviews,
                        NextDeparture = tour.NextDeparture,
                    });
                }
            }
        }

        return result;
    }

    #endregion
}
