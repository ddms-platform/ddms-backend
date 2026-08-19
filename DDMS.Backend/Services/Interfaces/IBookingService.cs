using DDMS.Backend.Models.DTOs.Booking;

namespace DDMS.Backend.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct);
    Task<BookingResponse> HoldAsync(Guid userId, CreateBookingRequest request, CancellationToken ct);
    Task<List<CabinAvailabilityResponse>> GetCabinAvailabilityAsync(Guid scheduleId, CancellationToken ct);
    Task<List<UserBookingListItemResponse>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    /// <summary>
    /// Chuyển booking sang đã thanh toán: trừ lượt mã giảm giá, bắn thông báo, gửi mail.
    /// Hàm này KHÔNG tự kiểm tra tiền — chỉ gọi sau khi đã đối chiếu với PayOS
    /// (xem <see cref="IBookingPaymentService"/>).
    /// </summary>
    Task MarkPaidAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Áp mã giảm giá lên booking đang chờ thanh toán và tính lại giá.
    /// Truyền <paramref name="code"/> null hoặc rỗng để gỡ mã.
    /// </summary>
    Task<BookingQuote> ApplyPromotionAsync(Guid bookingId, Guid userId, string? code, CancellationToken ct);
    Task<CancelBookingResult> CancelAsync(Guid bookingId, Guid userId, CancellationToken ct);
    Task<CheckInBookingResponse> CheckInAsync(CheckInBookingRequest request, CancellationToken ct);

    /// <summary>Huỷ các booking giữ chỗ đã quá hạn. Trả về số booking bị huỷ. Dùng cho background worker.</summary>
    Task<int> CancelExpiredHoldsAsync(CancellationToken ct);

    /// <summary>
    /// Chuyển đơn đã trả tiền sang "đã hoàn thành" khi chuyến đã chạy xong.
    /// Mở khoá luồng đánh giá: ReviewRepository chỉ nhỏn đơn ở trạng thái này.
    /// </summary>
    Task<int> CompleteFinishedToursAsync(CancellationToken ct);

    /// <summary>Gửi email nhắc cho các booking B2B sắp hết hạn giữ chỗ. Trả về số email đã gửi.</summary>
    Task<int> SendHoldRemindersAsync(CancellationToken ct);
}
