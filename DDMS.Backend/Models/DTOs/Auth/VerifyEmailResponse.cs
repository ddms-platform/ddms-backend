namespace DDMS.Backend.Models.DTOs.Auth;

public class VerifyEmailResponse
{
    public string message { get; set; } = string.Empty;
    public bool alreadyVerified { get; set; }
}
