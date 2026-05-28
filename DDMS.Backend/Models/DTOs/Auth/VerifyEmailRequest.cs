namespace DDMS.Backend.Models.DTOs.Auth;

public class VerifyEmailRequest
{
    public string token { get; set; } = string.Empty;
}
