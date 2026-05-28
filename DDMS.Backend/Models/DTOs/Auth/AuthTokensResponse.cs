namespace DDMS.Backend.Models.DTOs.Auth;

public class AuthTokensResponse
{
    public string token { get; set; } = string.Empty;
    public string refreshToken { get; set; } = string.Empty;
    public bool authenticated { get; set; }
    public int expiresInSeconds { get; set; }
}
