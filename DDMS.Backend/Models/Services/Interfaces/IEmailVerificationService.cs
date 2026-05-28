namespace DDMS.Backend.Models.Services.Interfaces;

public interface IEmailVerificationService
{
    Task<string> SendVerificationLinkAsync(string email);
    Task<string> VerifyByTokenAsync(string token);
}
