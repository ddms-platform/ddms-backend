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

    /// <summary>
    /// CHỈ DÙNG KHI DEV/DEMO: đánh dấu đơn đã trả tiền mà không qua PayOS.
    ///
    /// Chỉ chạy khi môi trường là Development, HOẶC người gọi có vai trò admin —
    /// admin cần đường này để demo trên production mà không phải chuyển tiền thật.
    /// Ngoài hai trường hợp đó thì ném, kể cả khi route vẫn được đăng ký.
    ///
    /// Vẫn giữ nguyên ràng buộc quyền sở hữu: chỉ giả lập được đơn của chính
    /// người gọi. Admin muốn demo thì tự đặt tour rồi tự bấm — không đụng được
    /// vào đơn của khách khác.
    /// </summary>
    /// <param name="isAdmin">Người gọi có vai trò admin hay không.</param>
    Task<BookingPaymentStatusResponse> SimulatePaidAsync(
        Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct);
}
