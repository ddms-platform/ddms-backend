using DDMS.Backend.Models.DTOs.OwnerToursDashboard;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerToursDashboardService
{
    Task<List<TourStatsItem>> GetStatsAsync(Guid ownerId, CancellationToken ct);
    Task<List<ScheduleListItem>> GetSchedulesAsync(Guid ownerId, int month, int year, CancellationToken ct);
    Task<List<RecentBookingItem>> GetRecentBookingsAsync(Guid ownerId, CancellationToken ct);
    Task<OwnerResourcesResponse> GetResourcesAsync(Guid ownerId, CancellationToken ct);
    Task CreateScheduleAsync(Guid ownerId, CreateScheduleRequest request, CancellationToken ct);
    Task<string> UpdateBookingStatusAsync(Guid ownerId, Guid bookingId, UpdateBookingStatusRequest request, CancellationToken ct);

    Task<string> CleanSeedDataAsync(CancellationToken ct);
    Task<string> RenameBoatsAsync(CancellationToken ct);
    Task<string> SeedToursAsync(CancellationToken ct);
}
