namespace DDMS.Backend.Models.DTOs.Auth;

public class LogoutRequest
{
    public string refreshToken { get; set; } = string.Empty;
}
