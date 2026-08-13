namespace DDMS.Backend.Configurations;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-flash-latest";
    public int MaxOutputTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Models tried in order after <see cref="Model"/> fails. Use the "-latest" aliases:
    /// pinned versions get retired by Google and every call then 404s.
    /// </summary>
    public string[] FallbackModels { get; set; } =
    [
        "gemini-flash-latest",
        "gemini-flash-lite-latest",
        "gemini-pro-latest",
    ];

    /// <summary>
    /// Reasoning budget for thinking models ("low" | "medium" | "high").
    /// Thinking tokens are billed against <see cref="MaxOutputTokens"/>, so the default
    /// "low" keeps the budget available for the actual answer. Empty = let the model decide.
    /// </summary>
    public string ThinkingLevel { get; set; } = "low";

    /// <summary>Configured model first, then the fallbacks, de-duplicated.</summary>
    public IEnumerable<string> ModelCandidates =>
        new[] { Model }
            .Concat(FallbackModels ?? [])
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct();

    /// <summary>Shared generationConfig payload for every Gemini call.</summary>
    public object BuildGenerationConfig() =>
        string.IsNullOrWhiteSpace(ThinkingLevel)
            ? new { temperature = Temperature, maxOutputTokens = MaxOutputTokens }
            : new
            {
                temperature = Temperature,
                maxOutputTokens = MaxOutputTokens,
                thinkingConfig = new { thinkingLevel = ThinkingLevel },
            };
}
