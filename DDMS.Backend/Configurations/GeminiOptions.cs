namespace DDMS.Backend.Configurations;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
    public int MaxOutputTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.7f;
}
