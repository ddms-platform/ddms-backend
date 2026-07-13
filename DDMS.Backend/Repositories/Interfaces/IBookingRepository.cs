using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<tour_schedule?> FindScheduleWithTourAsync(Guid scheduleId, CancellationToken ct);
    Task<tour_schedule?> FindScheduleWithCabinsAsync(Guid scheduleId, CancellationToken ct);
    Task<Dictionary<Guid, int>> GetBookedCabinQuantitiesAsync(Guid scheduleId, CancellationToken ct);
    Task<bool> HasActiveBookingForTourDateAsync(
        Guid userId,
        Guid tourId,
        DateTime startOfDay,
        DateTime endOfDay,
        CancellationToken ct);
    void AddBooking(booking entity);
    void AddBookingCabin(booking_cabin entity);
    void AddBookingService(booking_service entity);
    Task<List<booking>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithScheduleAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithDetailsAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindBookingForCheckInByIdAsync(Guid id, CancellationToken ct);
    Task<booking?> FindBookingForCheckInByCodeAsync(string codePrefix, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
