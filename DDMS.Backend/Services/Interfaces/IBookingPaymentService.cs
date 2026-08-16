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
    /// DÙNG ĐỂ DEMO: đánh dấu đơn đã trả tiền mà không qua PayOS.
    ///
    /// Mở cho mọi tài khoản đã đăng nhập, không phân biệt vai trò — đây là yêu
    /// cầu của kỳ bảo vệ, để bấm một cái là đơn xác nhận ngay mà không phải
    /// chuyển tiền thật.
    ///
    /// CẢNH BÁO: đây là đường duy nhất đánh dấu đã-trả-tiền mà không có tiền
    /// thật. Để mở như vậy nghĩa là bất kỳ khách nào cũng tự xác nhận được đơn
    /// của mình. Khoá lại bằng cách trả điều kiện môi trường/vai trò vào đây.
    ///
    /// Ràng buộc duy nhất còn giữ: chỉ giả lập được đơn của CHÍNH người gọi —
    /// không ai đụng được vào đơn của người khác.
    /// </summary>
    Task<BookingPaymentStatusResponse> SimulatePaidAsync(
        Guid bookingId, Guid userId, CancellationToken ct);
}
