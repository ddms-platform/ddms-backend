using DDMS.Backend.Models.DTOs.OwnerToursDashboard;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IOwnerToursDashboardRepository
{
    Task<List<TourStatsItem>> GetTourStatsAsync(Guid ownerId, CancellationToken ct);
    Task<List<ScheduleListItem>> GetSchedulesAsync(Guid ownerId, int month, int year, CancellationToken ct);
    Task<List<RecentBookingItem>> GetRecentBookingsAsync(Guid ownerId, int take, CancellationToken ct);
    Task<List<OwnerBoatResource>> GetOwnerResourcesAsync(Guid ownerId, CancellationToken ct);

    Task<boat?> FindOwnerBoatAsync(Guid boatId, Guid ownerId, CancellationToken ct);
    Task<tour?> FindTourAsync(Guid tourId, CancellationToken ct);
    Task<bool> HasTourScheduleOverlapAsync(Guid ownerId, Guid tourId, DateTime start, DateTime end, CancellationToken ct);
    Task<bool> HasScheduleOverlapAsync(Guid boatId, DateTime start, DateTime end, CancellationToken ct);
    void AddSchedule(tour_schedule schedule);

    Task<booking?> FindOwnerBookingWithDetailsAsync(Guid bookingId, Guid ownerId, CancellationToken ct);

    Task<List<tour_schedule>> GetSchedulesByDayMonthAsync(int day, int month, CancellationToken ct);
    void RemoveSchedules(IEnumerable<tour_schedule> schedules);
    Task<List<boat>> GetAllBoatsOrderedAsync(CancellationToken ct);
    Task<boat?> GetFirstBoatAsync(CancellationToken ct);
    void AddTours(IEnumerable<tour> tours);

    Task SaveChangesAsync(CancellationToken ct);
}
