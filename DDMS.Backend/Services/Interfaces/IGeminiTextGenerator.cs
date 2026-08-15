namespace DDMS.Backend.Services.Interfaces;

/// <summary>
/// Gọi Gemini sinh văn bản từ một prompt. Tách riêng khỏi IAiService (vốn gắn
/// với hội thoại và người dùng) để các tính năng nền như cào tin dùng lại được,
/// và để test được mà không cần gọi mạng thật.
/// </summary>
public interface IGeminiTextGenerator
{
    Task<string?> GenerateTextAsync(string prompt, CancellationToken ct);
}
