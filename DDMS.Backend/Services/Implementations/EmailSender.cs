using System.Net;
using System.Net.Mail;
using DDMS.Backend.Configurations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

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
        await SendHtmlEmailAsync(toEmail, subject, body, "Verification", verificationLink);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, int expiryMinutes)
    {
        var subject = "DDMS - Đặt lại mật khẩu";
        var body = BuildPasswordResetEmailHtml(resetLink, expiryMinutes);
        await SendHtmlEmailAsync(toEmail, subject, body, "Password reset", resetLink);
    }

    private async Task SendHtmlEmailAsync(
        string toEmail,
        string subject,
        string body,
        string logLabel,
        string devLink)
    {
        if (!_emailOptions.useSmtp)
        {
            _logger.LogInformation("{Label} email to {Email}: {Link}", logLabel, toEmail, devLink);
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
            _logger.LogInformation("{Label} email sent to {Email}", logLabel, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {Label} email to {Email}", logLabel, toEmail);
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

    private static string BuildPasswordResetEmailHtml(string resetLink, int expiryMinutes)
    {
        var safeLink = WebUtility.HtmlEncode(resetLink);

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
                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
              </p>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Nhấn vào nút bên dưới để tạo mật khẩu mới:
              </p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:0 40px 28px;"">
              <a href=""{safeLink}""
                 style=""display:inline-block;background-color:#00F0FF;color:#0A192F;font-weight:700;font-size:15px;text-decoration:none;padding:14px 36px;border-radius:10px;"">
                Đặt lại mật khẩu
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
                  <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</li>
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

    public async Task SendOwnerRegistrationSuccessEmailAsync(string toEmail, string ownerName, Models.DTOs.Auth.OwnerRegistrationRequest request, string language)
    {
        bool isEn = language == "en";
        var subject = isEn ? "Owner Registration Successful - DDMS" : "Đăng ký Chủ thuyền thành công - DDMS";

        string vesselsHtml = "";
        foreach(var v in request.Vessels) {
            vesselsHtml += $"<li><strong>{(isEn ? "Vessel Name" : "Tên thuyền")}:</strong> {v.Name} ({(isEn ? "Type" : "Loại")}: {v.Type})</li>";
        }

        string body = $@"<!DOCTYPE html>
<html lang=""{(isEn ? "en" : "vi")}"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">{(isEn ? "Dear" : "Kính chào")} {ownerName},</p>
              <p style=""margin:0 0 16px;color:#3c4858;font-size:15px;line-height:1.6;"">
                {(isEn ? "Thank you for submitting your request to become a Boat Owner at DDMS." : "Cảm ơn bạn đã gửi yêu cầu đăng ký trở thành Chủ thuyền tại DDMS.")}
              </p>
              <div style=""background-color:#f8fafc;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 12px;color:#0A192F;font-size:14px;"">{(isEn ? "Registration Details" : "Thông tin đăng ký")}:</h3>
                <ul style=""margin:0;padding-left:20px;color:#3c4858;font-size:14px;line-height:1.6;"">
                   <li><strong>{(isEn ? "Full Name" : "Họ và tên")}:</strong> {request.FullName}</li>
                   <li><strong>{(isEn ? "Phone Number" : "Số điện thoại")}:</strong> {request.Phone}</li>
                   <li><strong>{(isEn ? "ID/Passport" : "CMND/CCCD/Hộ chiếu")}:</strong> {request.LicenseNumber}</li>
                   <li><strong>{(isEn ? "Address" : "Địa chỉ")}:</strong> {request.Address}</li>
                   <li><strong>{(isEn ? "Registered Vessels" : "Số lượng thuyền đăng ký")}:</strong> {request.Vessels.Count}
                       <ul style=""margin-top:8px;padding-left:20px;"">{vesselsHtml}</ul>
                   </li>
                </ul>
              </div>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;text-align:left;"">
                {(isEn ? "Your profile and vessel information have been successfully recorded in the system. The administration board will review the information and respond to you as soon as possible." : "Hồ sơ và thông tin du thuyền của bạn đã được ghi nhận thành công trên hệ thống. Ban quản trị sẽ tiến hành kiểm duyệt thông tin và phản hồi kết quả cho bạn trong thời gian sớm nhất.")}
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
              <p style=""margin:0;color:#8a94a6;font-size:13px;line-height:1.6;"">
                {(isEn ? "Best regards," : "Trân trọng,")}<br />{(isEn ? "DDMS Development Team" : "Đội ngũ phát triển hệ thống DDMS")}
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        await SendHtmlEmailAsync(toEmail, subject, body, "Owner Registration", "");
    }

    public async Task SendBookingStatusEmailAsync(
        string toEmail, 
        string customerName, 
        string bookingId, 
        string tourName, 
        string boatName, 
        DateTime tourTime, 
        decimal totalPrice, 
        string status, 
        string? cancelReason)
    {
        var statusUpper = status.ToUpper();
        var subject = $"DDMS - Cập nhật trạng thái đơn đặt chỗ #{bookingId} [{statusUpper}]";
        
        string statusColor = status.ToLower() switch
        {
            "confirmed" => "#00C49F",
            "completed" => "#0088FE",
            "cancelled" => "#FF8042",
            _ => "#94a3b8"
        };

        string statusText = status.ToLower() switch
        {
            "confirmed" => "ĐÃ XÁC NHẬN",
            "completed" => "ĐÃ HOÀN THÀNH",
            "cancelled" => "ĐÃ HỦY / TỪ CHỐI",
            _ => statusUpper
        };

        string cancelReasonHtml = "";
        if (status.ToLower() == "cancelled" && !string.IsNullOrWhiteSpace(cancelReason))
        {
            cancelReasonHtml = $@"
              <div style=""background-color:#fff1f0;border:1px solid #ffa39e;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 8px;color:#cf1322;font-size:14px;"">Lý do hủy đơn:</h3>
                <p style=""margin:0;color:#3c4858;font-size:14px;line-height:1.6;"">
                  {cancelReason}
                </p>
              </div>";
        }

        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Kính chào {customerName},</p>
              <p style=""margin:0 0 20px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Trạng thái đơn đặt chỗ **#{bookingId}** của bạn đã được cập nhật thành: 
                <span style=""display:inline-block;padding:4px 12px;font-weight:bold;font-size:14px;color:#ffffff;background-color:{statusColor};border-radius:4px;margin-left:4px;"">
                  {statusText}
                </span>
              </p>

              {cancelReasonHtml}

              <div style=""background-color:#f8fafc;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 12px;color:#0A192F;font-size:14px;"">Chi tiết đơn đặt chỗ:</h3>
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;color:#3c4858;"">
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;width:130px;"">Mã đơn:</td>
                    <td style=""padding:6px 0;font-family:monospace;font-weight:bold;color:#0A192F;"">#{bookingId}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Dịch vụ/Tour:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{tourName}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Tàu:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{boatName}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Thời gian khởi hành:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{tourTime.ToString("HH:mm dd/MM/yyyy")}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;border-top:1px solid #eef1f6;margin-top:6px;padding-top:12px;"">Tổng thanh toán:</td>
                    <td style=""padding:6px 0;font-weight:bold;color:#cf1322;font-size:16px;border-top:1px solid #eef1f6;padding-top:12px;"">{totalPrice.ToString("N0")} VNĐ</td>
                  </tr>
                </table>
              </div>

              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;text-align:left;"">
                Cảm ơn bạn đã đồng hành cùng hệ thống đặt chỗ du thuyền DDMS. Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline hỗ trợ.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Booking Status Update", "");
    }

    public async Task SendServiceRegistrationSuccessEmailAsync(
        string toEmail, 
        string ownerName, 
        string serviceName, 
        string boatName, 
        decimal basePrice)
    {
        var subject = "Đăng ký dịch vụ trên tàu thành công - DDMS";
        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Kính chào {ownerName},</p>
              <p style=""margin:0 0 16px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Chúc mừng bạn đã đăng ký dịch vụ/tour thành công cho tàu của mình trên hệ thống DDMS.
              </p>
              <div style=""background-color:#f8fafc;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 12px;color:#0A192F;font-size:14px;"">Thông tin dịch vụ:</h3>
                <ul style=""margin:0;padding-left:20px;color:#3c4858;font-size:14px;line-height:1.6;"">
                   <li><strong>Tên dịch vụ/tour:</strong> {serviceName}</li>
                   <li><strong>Tên tàu đăng ký:</strong> {boatName}</li>
                   <li><strong>Giá cơ bản:</strong> {basePrice.ToString("N0")} VNĐ</li>
                </ul>
              </div>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;text-align:left;"">
                Dịch vụ của bạn đã được chuyển tới Ban quản trị để kiểm duyệt. Chúng tôi sẽ thông báo cho bạn ngay sau khi dịch vụ được phê duyệt và kích hoạt trên hệ thống.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Service Registration", "");
    }

    public async Task SendOwnerVerificationApprovedEmailAsync(string toEmail, string ownerName)
    {
        var subject = "DDMS - Hồ sơ chủ thuyền của bạn đã được duyệt";
        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Kính chào {ownerName},</p>
              <p style=""margin:0 0 16px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Chúc mừng bạn! Yêu cầu đăng ký trở thành Chủ thuyền tại DDMS của bạn đã được duyệt thành công.
              </p>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Tài khoản của bạn đã được cấp quyền Chủ thuyền (Vessel Owner). Bây giờ bạn đã có thể đăng nhập vào bảng điều khiển dành cho Chủ thuyền để quản lý các tàu thuyền, chuyến đi, dịch vụ và theo dõi doanh thu của mình.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Owner Verification Approved", "");
    }

    public async Task SendBoatDockAssignmentEmailAsync(
        string toEmail,
        string ownerName,
        string boatName,
        string dockName,
        string slipCode,
        DateTime startTime,
        DateTime endTime)
    {
        var subject = "DDMS - Thông báo xếp bến đỗ du thuyền";
        string slipHtml = string.IsNullOrEmpty(slipCode) ? "" : $"<li><strong>Vị trí đỗ (Slip):</strong> {slipCode}</li>";
        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Kính chào {ownerName},</p>
              <p style=""margin:0 0 16px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Chúng tôi xin thông báo ban quản lý bến cảng đã sắp xếp chỗ neo đậu cho du thuyền của bạn.
              </p>
              <div style=""background-color:#f8fafc;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 12px;color:#0A192F;font-size:14px;"">Thông tin xếp bến:</h3>
                <ul style=""margin:0;padding-left:20px;color:#3c4858;font-size:14px;line-height:1.6;"">
                   <li><strong>Tên du thuyền:</strong> {boatName}</li>
                   <li><strong>Bến cảng/Cầu cảng:</strong> {dockName}</li>
                   {slipHtml}
                   <li><strong>Thời gian bắt đầu:</strong> {startTime.ToString("HH:mm dd/MM/yyyy")}</li>
                   <li><strong>Thời gian kết thúc:</strong> {endTime.ToString("HH:mm dd/MM/yyyy")}</li>
                </ul>
              </div>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;text-align:left;"">
                Vui lòng di chuyển du thuyền của bạn vào đúng vị trí neo đậu đã được sắp xếp trong thời gian quy định. Xin cảm ơn!
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Boat Dock Assignment Notice", "");
    }

    public async Task SendMaintenanceStatusEmailAsync(
        string toEmail, 
        string ownerName, 
        string boatName, 
        string serviceName, 
        string status, 
        decimal price)
    {
        var statusColor = status.ToLower() == "approved" ? "#10B981" : "#EF4444";
        var statusText = status.ToLower() == "approved" ? "ĐÃ DUYỆT" : "TỪ CHỐI";
        var subject = $"DDMS - Kết quả duyệt dịch vụ bảo trì [{statusText}]";

        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Kính chào {ownerName},</p>
              <p style=""margin:0 0 20px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Yêu cầu đăng ký dịch vụ bảo trì của bạn đã được cập nhật trạng thái duyệt từ ban quản trị hệ thống:
                <span style=""display:inline-block;padding:4px 12px;font-weight:bold;font-size:14px;color:#ffffff;background-color:{statusColor};border-radius:4px;margin-left:4px;"">
                  {statusText}
                </span>
              </p>

              <div style=""background-color:#f8fafc;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 12px;color:#0A192F;font-size:14px;"">Thông tin yêu cầu bảo trì:</h3>
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;color:#3c4858;"">
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;width:130px;"">Tên du thuyền:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{boatName}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Dịch vụ bảo trì:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{serviceName}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Chi phí dịch vụ:</td>
                    <td style=""padding:6px 0;color:#cf1322;font-weight:bold;"">{price.ToString("N0")} VNĐ</td>
                  </tr>
                </table>
              </div>

              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;text-align:left;"">
                {(status.ToLower() == "approved" ? "Chi phí của dịch vụ bảo trì này đã được cộng vào công nợ tháng của bạn. Vui lòng thanh toán vào kỳ đối soát tiếp theo." : "Yêu cầu bảo trì bị từ chối. Nếu bạn có bất kỳ thắc mắc nào, vui lòng phản hồi lại email này để được giải đáp.")}
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Maintenance Approval Status Notice", "");
    }

    public async Task SendWithdrawalStatusEmailAsync(
        string toEmail, 
        string userName, 
        decimal amount, 
        string bankName, 
        string accountNumber, 
        string status)
    {
        var statusColor = status.ToLower() == "approved" ? "#10B981" : "#EF4444";
        var statusText = status.ToLower() == "approved" ? "THÀNH CÔNG" : "BỊ TỪ CHỐI";
        var subject = $"DDMS - Kết quả duyệt yêu cầu rút tiền [{statusText}]";

        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Chào bạn {userName},</p>
              <p style=""margin:0 0 20px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Yêu cầu rút tiền từ ví tài khoản của bạn trên hệ thống đã được cập nhật kết quả duyệt:
                <span style=""display:inline-block;padding:4px 12px;font-weight:bold;font-size:14px;color:#ffffff;background-color:{statusColor};border-radius:4px;margin-left:4px;"">
                  {statusText}
                </span>
              </p>

              <div style=""background-color:#f8fafc;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <h3 style=""margin:0 0 12px;color:#0A192F;font-size:14px;"">Thông tin giao dịch rút tiền:</h3>
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;color:#3c4858;"">
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;width:130px;"">Số tiền rút:</td>
                    <td style=""padding:6px 0;color:#cf1322;font-weight:bold;font-size:16px;"">{amount.ToString("N0")} VNĐ</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Ngân hàng nhận:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{bankName}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;font-weight:bold;color:#6b7280;"">Số tài khoản:</td>
                    <td style=""padding:6px 0;color:#0A192F;"">{accountNumber}</td>
                  </tr>
                </table>
              </div>

              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;text-align:left;"">
                {(status.ToLower() == "approved" ? "Số tiền rút đã được chuyển khoản thủ công về tài khoản ngân hàng của bạn. Hãy kiểm tra số dư ngân hàng." : "Yêu cầu rút tiền của bạn bị từ chối từ ban quản trị. Số tiền tương ứng đã được hệ thống hoàn lại tự động vào số dư ví của bạn.")}
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Withdrawal Approval Status Notice", "");
    }

    public async Task SendNewChatMessageEmailAsync(
        string toEmail,
        string recipientName,
        string senderName,
        string messageBody,
        string viewChatLink)
    {
        var subject = $"DDMS - Tin nhắn mới từ {senderName}";
        var body = BuildNewChatMessageEmailHtml(recipientName, senderName, messageBody, viewChatLink);
        await SendHtmlEmailAsync(toEmail, subject, body, "New Chat Message Notice", viewChatLink);
    }

    private static string BuildNewChatMessageEmailHtml(
        string recipientName,
        string senderName,
        string messageBody,
        string viewChatLink)
    {
        var safeLink = WebUtility.HtmlEncode(viewChatLink);
        var safeMessage = WebUtility.HtmlEncode(messageBody);

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
              <h1 style=""margin:0;color:#ffffff;font-size:22px;letter-spacing:-0.3px;"">DDMS Chat</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;font-weight:bold;"">Chào {recipientName},</p>
              <p style=""margin:0 0 16px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Bạn có tin nhắn mới từ <strong>{senderName}</strong>:
              </p>
              <div style=""background-color:#f8fafc;border-left:4px solid #ff385c;padding:16px;border-radius:4px;margin-bottom:24px;font-style:italic;color:#4a5568;text-align:left;"">
                ""{safeMessage}""
              </div>
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Nhấn vào nút bên dưới để xem chi tiết cuộc trò chuyện và trả lời:
              </p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:0 40px 28px;"">
              <a href=""{safeLink}""
                 style=""display:inline-block;background-color:#ff385c;color:#ffffff;font-weight:700;font-size:15px;text-decoration:none;padding:14px 36px;border-radius:10px;box-shadow:0 4px 12px rgba(255,56,92,0.24);"">
                Xem cuộc trò chuyện
              </a>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

    public async Task SendScheduleChangeEmailAsync(
        string toEmail,
        string customerName,
        string bookingId,
        string tourName,
        DateTime oldTime,
        DateTime newTime)
    {
        var subject = $"DDMS - Thay đổi lịch trình tour [{bookingId}]";
        var oldTimeStr = oldTime.ToString("HH:mm dd/MM/yyyy");
        var newTimeStr = newTime.ToString("HH:mm dd/MM/yyyy");
        var body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fb;font-family:Segoe UI,Roboto,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fb;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(10,25,47,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#0A192F 0%,#112240 60%,#0d2847 100%);padding:32px;text-align:center;"">
              <h1 style=""margin:0;color:#00F0FF;font-size:22px;letter-spacing:-0.3px;"">DDMS</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:36px 40px 8px;"">
              <p style=""margin:0 0 16px;color:#0A192F;font-size:16px;"">Kính chào {customerName},</p>
              <p style=""margin:0 0 20px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Chúng tôi xin thông báo lịch khởi hành của tour <strong>{tourName}</strong> (Mã đặt chỗ: <strong>#{bookingId}</strong>) đã thay đổi vì lý do bất khả kháng (thời tiết/kỹ thuật).
              </p>
              
              <div style=""background-color:#fff2e8;border:1px solid #ffd591;padding:16px;border-radius:8px;margin-bottom:24px;text-align:left;"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;color:#3c4858;"">
                  <tr>
                    <td style=""padding:6px 0;color:#fa8c16;font-weight:bold;width:150px;"">Giờ khởi hành CŨ:</td>
                    <td style=""padding:6px 0;text-decoration:line-through;color:#8c8c8c;"">{oldTimeStr}</td>
                  </tr>
                  <tr>
                    <td style=""padding:6px 0;color:#52c41a;font-weight:bold;width:150px;"">Giờ khởi hành MỚI:</td>
                    <td style=""padding:6px 0;color:#237804;font-weight:bold;font-size:16px;"">{newTimeStr}</td>
                  </tr>
                </table>
              </div>
              
              <p style=""margin:0 0 24px;color:#3c4858;font-size:15px;line-height:1.6;"">
                Rất mong bạn thông cảm cho sự bất tiện này. Quý khách vui lòng có mặt tại bến cảng trước giờ khởi hành mới ít nhất 15 phút để làm thủ tục.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#f8fafc;padding:24px 40px;text-align:center;"">
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

        await SendHtmlEmailAsync(toEmail, subject, body, "Schedule Change Notice", "");
    }
}
