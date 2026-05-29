namespace DDMS.Backend.Models.Services.Interfaces;

public record EmailVerificationResult(string email, bool alreadyVerified);

public interface IEmailVerificationService
{
    Task<string> SendVerificationLinkAsync(string email);
    Task<EmailVerificationResult> VerifyByTokenAsync(string token);
}
