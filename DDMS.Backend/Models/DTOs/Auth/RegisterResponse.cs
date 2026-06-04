namespace DDMS.Backend.Models.DTOs.Auth;

public class RegisterResponse
{
    public bool requiresEmailVerification { get; set; }
    public string email { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public string? verificationLink { get; set; }
}
