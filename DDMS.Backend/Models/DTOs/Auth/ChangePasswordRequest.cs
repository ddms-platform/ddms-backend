namespace DDMS.Backend.Models.DTOs.Auth;

public class ChangePasswordRequest
{
    public string currentPassword { get; set; } = string.Empty;
    public string newPassword { get; set; } = string.Empty;
    public string confirmPassword { get; set; } = string.Empty;
}
