using System.Net;
using System.Net.Mail;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Models.Services.Implementations;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailOptions> emailOptions, ILogger<EmailSender> logger)
    {
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendVerificationLinkEmailAsync(string toEmail, string verificationLink, int expiryHours)
    {
        var subject = "DDMS - Verify your email";
        var body = $"Click the link below to verify your email:\n{verificationLink}\n\nThis link expires in {expiryHours} hours.";

        if (!_emailOptions.useSmtp)
        {
            _logger.LogInformation("Verification email to {Email}: {Link}", toEmail, verificationLink);
            await Task.CompletedTask;
            return;
        }

        using var client = new SmtpClient(_emailOptions.smtpHost, _emailOptions.smtpPort)
        {
            EnableSsl = _emailOptions.enableSsl,
            Credentials = new NetworkCredential(_emailOptions.smtpUser, _emailOptions.smtpPassword)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_emailOptions.fromAddress, _emailOptions.fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}
