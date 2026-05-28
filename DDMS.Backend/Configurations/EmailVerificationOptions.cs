namespace DDMS.Backend.Configurations;

public class EmailVerificationOptions
{
    public const string SectionName = "EmailVerification";

    public string frontendBaseUrl { get; set; } = "http://localhost:5173";
    public string verifyEmailPath { get; set; } = "/verify-email";
    public int tokenExpiryHours { get; set; } = 24;
    public int resendCooldownSeconds { get; set; } = 60;
    public int maxRequestsPerHour { get; set; } = 5;
}
