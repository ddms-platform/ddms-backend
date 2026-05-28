namespace DDMS.Backend.Configurations;

public class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "DdmsCorsPolicy";

    public string[] AllowedOrigins { get; set; } = [];
}
