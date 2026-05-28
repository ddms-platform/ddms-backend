namespace DDMS.Backend.Configurations;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string issuer { get; set; } = string.Empty;
    public string audience { get; set; } = string.Empty;
    public string secretKey { get; set; } = string.Empty;
    public int accessTokenMinutes { get; set; } = 60;
    public int refreshTokenDays { get; set; } = 7;
}
