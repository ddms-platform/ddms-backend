namespace DDMS.Backend.Models.DTOs.Auth;

public class MessageResponse
{
    public string message { get; set; } = string.Empty;
    public string? verificationLink { get; set; }
}
