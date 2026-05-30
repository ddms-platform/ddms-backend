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

    public async Task SendVerificationLinkEmailAsync(string toEmail, string verificationLink, int expiryMinutes)
    {
        var subject = "DDMS - Xác thực email của bạn";
        var body = BuildVerificationEmailHtml(verificationLink, expiryMinutes);

        if (!_emailOptions.useSmtp)
        {
            _logger.LogInformation("Verification email to {Email}: {Link}", toEmail, verificationLink);
            await Task.CompletedTask;
            return;
        }

        if (string.IsNullOrWhiteSpace(_emailOptions.smtpHost)
            || string.IsNullOrWhiteSpace(_emailOptions.smtpUser)
            || string.IsNullOrWhiteSpace(_emailOptions.smtpPassword)
            || string.IsNullOrWhiteSpace(_emailOptions.fromAddress))
        {
            _logger.LogError(
                "SMTP is enabled but Email settings are incomplete. Configure Gmail via User Secrets (see SECRETS.md).");
            throw new InvalidOperationException(
                "Email SMTP is not configured. Set Email:* values via dotnet user-secrets (see SECRETS.md).");
        }

        try
        {
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
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Verification email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", toEmail);
            throw;
        }
    }

    private static string BuildVerificationEmailHtml(string verificationLink, int expiryMinutes)
    {
        var safeLink = WebUtility.HtmlEncode(verificationLink);

        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#ffffff;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Chào bạn,</p>
              <p style=""margin:0 0 16px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Cảm ơn bạn đã đăng ký tài khoản trên hệ thống của chúng tôi.
              </p>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Để hoàn tất quá trình đăng ký, vui lòng nhấn vào nút bên dưới để xác thực email của bạn:
              </p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:0 40px 28px;"">
              <a href=""{safeLink}""
                 style=""display:inline-block;background-color:#00F0FF;color:#0A192F;font-weight:700;font-size:15px;text-decoration:none;padding:14px 36px;border-radius:10px;"">
                Xác thực email
              </a>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 40px 8px;"">
              <p style=""margin:0 0 8px;color:#8a94a6;font-size:13px;line-height:1.6;"">
                Hoặc sao chép đường link sau vào trình duyệt:
              </p>
              <p style=""margin:0 0 24px;word-break:break-all;"">
                <a href=""{safeLink}"" style=""color:#0d6efd;font-size:13px;"">{safeLink}</a>
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 40px 28px;"">
              <div style=""border-top:1px solid #eef1f6;padding-top:20px;"">
                <p style=""margin:0 0 6px;color:#3c4858;font-size:13px;line-height:1.6;"">
                  <strong>Lưu ý:</strong>
                </p>
                <ul style=""margin:0;padding-left:18px;color:#8a94a6;font-size:13px;line-height:1.7;"">
                  <li>Link này sẽ hết hạn sau {expiryMinutes} phút.</li>
                  <li>Nếu bạn không thực hiện đăng ký, vui lòng bỏ qua email này.</li>
                </ul>
              </div>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;"">
              <p style=""margin:0;color:#8a94a6;font-size:13px;line-height:1.6;"">
                Trân trọng,<br />Đội ngũ phát triển hệ thống DDMS
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
