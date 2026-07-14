using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<tour_schedule?> FindScheduleWithTourAsync(Guid scheduleId, CancellationToken ct);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken ct);
    Task<int> CancelExpiredHoldsAsync(DateTime now, string reason, CancellationToken ct);
    Task<List<booking>> GetHoldsNeedingReminderAsync(DateTime now, DateTime remindBefore, string agentRole, CancellationToken ct);
    void AddBooking(booking entity);
    void AddBookingCabin(booking_cabin entity);
    void AddBookingService(booking_service entity);
    Task<List<booking>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithScheduleAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithDetailsAsync(Guid id, Guid userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
