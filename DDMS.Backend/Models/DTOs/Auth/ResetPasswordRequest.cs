namespace DDMS.Backend.Models.DTOs.Auth;

public class ResetPasswordRequest
{
    public string token { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string confirmPassword { get; set; } = string.Empty;
}
