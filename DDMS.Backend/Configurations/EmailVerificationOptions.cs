namespace DDMS.Backend.Configurations;

public class EmailVerificationOptions
{
    public const string SectionName = "EmailVerification";

    public string frontendBaseUrl { get; set; } = "http://localhost:5173";
    public string verifyEmailPath { get; set; } = "/verify-email";
    public string resetPasswordPath { get; set; } = "/reset-password";
    public int tokenExpiryMinutes { get; set; } = 15;
    public int resendCooldownSeconds { get; set; } = 60;
    public int maxRequestsPerHour { get; set; } = 5;
}
