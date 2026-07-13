using DDMS.Backend.Models.DTOs.Booking;

namespace DDMS.Backend.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct);
    Task<List<CabinAvailabilityResponse>> GetCabinAvailabilityAsync(Guid scheduleId, CancellationToken ct);
    Task<List<UserBookingListItemResponse>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    Task ConfirmPaymentAsync(Guid bookingId, Guid userId, CancellationToken ct);
    Task<CancelBookingResult> CancelAsync(Guid bookingId, Guid userId, CancellationToken ct);
    Task<CheckInBookingResponse> CheckInAsync(CheckInBookingRequest request, CancellationToken ct);
}
