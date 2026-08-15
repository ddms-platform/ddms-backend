using System.Text;
using System.Text.Json;
using DDMS.Backend.Configurations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

/// <summary>
/// Gọi generateContent của Gemini, thử lần lượt các model trong ModelCandidates
/// cho tới khi có model trả về kết quả — giống cách GeminiAiService đang làm.
/// </summary>
public class GeminiTextGenerator : IGeminiTextGenerator
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiTextGenerator> _logger;

    public GeminiTextGenerator(
        HttpClient http,
        IOptions<GeminiOptions> options,
        ILogger<GeminiTextGenerator> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> GenerateTextAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException(
                "Gemini:ApiKey chưa được cấu hình, không sinh được nội dung.");

        var body = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = _options.BuildGenerationConfig()
        };
        var payload = JsonSerializer.Serialize(body);

        foreach (var model in _options.ModelCandidates)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var endpoint = "https://generativelanguage.googleapis.com/v1beta/models/"
                             + $"{model}:generateContent?key={_options.ApiKey}";
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var res = await _http.PostAsync(endpoint, content, ct);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini {Model} trả về {Status}", model, res.StatusCode);
                    continue;
                }

                using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
                if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
                    || candidates.GetArrayLength() == 0) continue;

                var text = candidates[0]
                    .GetProperty("content").GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gọi Gemini {Model} thất bại", model);
            }
        }

        return null;
    }
}
