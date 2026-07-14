using DDMS.Backend.Models.DTOs.Booking;

namespace DDMS.Backend.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct);
    Task<BookingResponse> HoldAsync(Guid userId, CreateBookingRequest request, CancellationToken ct);
    Task<List<UserBookingListItemResponse>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    Task ConfirmPaymentAsync(Guid bookingId, Guid userId, CancellationToken ct);
    Task<CancelBookingResult> CancelAsync(Guid bookingId, Guid userId, CancellationToken ct);

    /// <summary>Huỷ các booking giữ chỗ đã quá hạn. Trả về số booking bị huỷ. Dùng cho background worker.</summary>
    Task<int> CancelExpiredHoldsAsync(CancellationToken ct);
}
