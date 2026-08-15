using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Services.Interfaces;

/// <summary>
/// Lớp mỏng bọc PayOSClient. Tồn tại để luồng thanh toán booking test được:
/// PayOSClient là class cụ thể, không mock nổi, mà đây lại đúng chỗ quyết định
/// một đơn có được coi là đã trả tiền hay không.
/// </summary>
public interface IPayOSGateway
{
    /// <summary>Đã cấu hình ClientId/ApiKey/ChecksumKey chưa.</summary>
    bool IsConfigured { get; }

    Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken ct);

    /// <summary>Tra trạng thái một đơn. Trả null nếu không hỏi được PayOS.</summary>
    Task<PaymentLink?> GetPaymentLinkAsync(long orderCode, CancellationToken ct);

    /// <summary>Xác minh chữ ký webhook. Trả null nếu chữ ký sai.</summary>
    Task<WebhookData?> VerifyWebhookAsync(Webhook body, CancellationToken ct);
}
