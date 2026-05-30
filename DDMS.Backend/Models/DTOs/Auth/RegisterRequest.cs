namespace DDMS.Backend.Models.DTOs.Auth;

public class RegisterRequest
{
    public string fullName { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string confirmPassword { get; set; } = string.Empty;
}
