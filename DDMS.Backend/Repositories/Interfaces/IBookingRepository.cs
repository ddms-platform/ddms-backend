using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<tour_schedule?> FindScheduleWithTourAsync(Guid scheduleId, CancellationToken ct);
    void AddBooking(booking entity);
    void AddBookingCabin(booking_cabin entity);
    void AddBookingService(booking_service entity);
    Task<List<booking>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithScheduleAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithDetailsAsync(Guid id, Guid userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
