using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IAdminDashboardRepository
{
    Task<int> CountUsersAsync(CancellationToken ct);
    Task<int> CountUsersBeforeAsync(DateTime before, CancellationToken ct);

    Task<int> CountToursByStatusAsync(IReadOnlyCollection<string> statuses, CancellationToken ct);
    Task<int> CountAllToursAsync(CancellationToken ct);
    Task<int> CountToursBeforeAsync(DateTime before, CancellationToken ct);

    Task<int> CountBoatsByStatusAsync(IReadOnlyCollection<string> statuses, CancellationToken ct);
    Task<int> CountActiveBoatsAsync(CancellationToken ct);
    Task<int> CountActiveBoatsBeforeAsync(DateTime before, CancellationToken ct);

    Task<decimal> SumRevenueAsync(IReadOnlyCollection<string> statuses, DateTime from, DateTime? toExclusive, CancellationToken ct);
    Task<int> CountBookingsByStatusAsync(IReadOnlyCollection<string> statuses, CancellationToken ct);

    Task<List<booking>> GetRecentBookingsAsync(int take, CancellationToken ct);
    Task<List<owner_profile>> GetPendingOwnerProfilesAsync(IReadOnlyCollection<string> statuses, int take, CancellationToken ct);

    Task<int> CountDocksAsync(CancellationToken ct);
    Task<int> CountActivePromotionsAsync(DateTime now, CancellationToken ct);
    Task<int> CountAuditLogsSinceAsync(DateTime since, CancellationToken ct);

    Task<List<tour>> GetAllToursWithCreatorAsync(CancellationToken ct);
    Task<int> CountCompletedBookingsForTourAsync(Guid tourId, CancellationToken ct);
    Task<decimal> SumCompletedRevenueForTourAsync(Guid tourId, CancellationToken ct);
    Task<owner_profile?> FindOwnerProfileByUserAsync(Guid userId, CancellationToken ct);
}
