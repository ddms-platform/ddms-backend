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
    }

    private class TourRagInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Duration { get; set; }
        public string? ImageUrl { get; set; }
        public string? BoatName { get; set; }
        public string? DepartureDock { get; set; }
        public string? Description { get; set; }
    }

    private async Task<RagContextData> BuildRagContextAsync()
    {
        var tours = await _context.tours
            .Where(t => t.status != "deleted")
            .Include(t => t.tour_schedules)
                .ThenInclude(s => s.boat)
            .Include(t => t.tour_images)
            .Take(15)
            .Select(t => new TourRagInfo
            {
                Id = t.id,
                Title = t.name,
                Price = t.price,
                Duration = $"{t.duration_minutes} phút",
                ImageUrl = t.tour_images.FirstOrDefault() != null ? t.tour_images.FirstOrDefault()!.image_url : null,
                BoatName = t.tour_schedules.FirstOrDefault() != null && t.tour_schedules.FirstOrDefault()!.boat != null ? t.tour_schedules.FirstOrDefault()!.boat.name : "Tàu Du Thuyền Sông Hàn",
                DepartureDock = t.location ?? "Bến du thuyền Sông Hàn",
                Description = t.description
            })
            .ToListAsync();

        var docks = await _context.docks
            .Select(d => d.name)
            .Take(5)
            .ToListAsync();

        return new RagContextData
        {
            Tours = tours,
            Docks = docks
        };
    }

    private async Task<string> CallGeminiApiAsync(RagContextData ragContext, List<ai_message> history, string currentPrompt)
    {
        try
        {
            var systemInstruction = new StringBuilder();
            systemInstruction.AppendLine("Bạn là Trợ lý AI Chuyên viên Tư vấn Du thuyền DDMS (Da Nang Dock Management System) tại Đà Nẵng.");
            systemInstruction.AppendLine("Hãy trả lời thân thiện, lịch sự, chuyên nghiệp bằng tiếng Việt.");
            systemInstruction.AppendLine("Bạn có quyền truy cập vào danh sách tour du thuyền thực tế đang hoạt động tại Đà Nẵng bên dưới:");
            systemInstruction.AppendLine("--- DANH SÁCH TOUR THỰC TẾ ---");

            foreach (var t in ragContext.Tours)
            {
                systemInstruction.AppendLine($"- ID: {t.Id} | Tên: {t.Title} | Giá: {t.Price:N0} VNĐ | Thời gian: {t.Duration} | Tàu: {t.BoatName} | Bến: {t.DepartureDock}");
            }
            systemInstruction.AppendLine("--------------------------------");
            systemInstruction.AppendLine("Khi du khách hỏi về tour phù hợp, hãy tư vấn chi tiết và ĐƯA TÊN TOUR CÙNG MÃ TOUR [ID: {guid}] vào câu trả lời để hệ thống hiển thị card tour.");

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
                generationConfig = new
                {
                    temperature = _options.Temperature,
                    maxOutputTokens = _options.MaxOutputTokens
                }
            };

            var modelsToTry = new[] { _options.Model, "gemini-1.5-flash-latest", "gemini-2.0-flash", "gemini-1.5-pro", "gemini-pro" }
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct();

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
                        DepartureLocation = tour.DepartureDock
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
                        DepartureLocation = tour.DepartureDock
                    });
                }
            }
        }

        return result;
    }

    #endregion
}
