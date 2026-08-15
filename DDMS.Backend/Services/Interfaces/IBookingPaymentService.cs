using DDMS.Backend.Models.DTOs.Billing;
using DDMS.Backend.Models.DTOs.Booking;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Services.Interfaces;

public interface IBookingPaymentService
{
    /// <summary>Tạo link thanh toán PayOS cho booking đang chờ trả tiền.</summary>
    Task<BookingPaymentInitResult> CreatePaymentLinkAsync(Guid bookingId, Guid userId, CancellationToken ct);

    /// <summary>
    /// Hỏi PayOS xem đơn đã được trả chưa rồi cập nhật booking theo kết quả đó.
    /// Đây là đường duy nhất phía khách có thể làm booking chuyển sang đã thanh toán —
    /// và nó không tin gì từ client ngoài id booking.
    /// </summary>
    Task<BookingPaymentStatusResponse> SyncStatusAsync(Guid bookingId, Guid userId, CancellationToken ct);

    /// <summary>Webhook PayOS gọi vào. Chữ ký được SDK xác minh trước khi ghi gì.</summary>
    Task<WebhookHandleResult> HandleWebhookAsync(Webhook body, CancellationToken ct);
}
