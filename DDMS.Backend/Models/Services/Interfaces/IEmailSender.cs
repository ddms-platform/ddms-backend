namespace DDMS.Backend.Models.Services.Interfaces;

public interface IEmailSender
{
    Task SendVerificationLinkEmailAsync(string toEmail, string verificationLink, int expiryMinutes);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, int expiryMinutes);
}
