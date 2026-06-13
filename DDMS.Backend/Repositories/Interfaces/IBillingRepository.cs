using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBillingRepository
{
    Task<List<booking>> GetOwnerRevenueBookingsAsync(Guid ownerId, CancellationToken ct);
    Task<List<boat_maintenance>> GetOwnerApprovedMaintenancesAsync(Guid ownerId, CancellationToken ct);
    Task<List<boat>> GetOwnerBoatsAsync(Guid ownerId, CancellationToken ct);
    Task<List<dock_schedule>> GetSchedulesForBoatsAsync(IReadOnlyCollection<Guid> boatIds, CancellationToken ct);
    Task<List<owner_payment>> GetOwnerPaymentsAsync(Guid ownerId, CancellationToken ct);
    Task<decimal> GetOwnerTotalPaidAsync(Guid ownerId, CancellationToken ct);
    Task AddPaymentAsync(owner_payment payment, CancellationToken ct);
    Task<owner_payment?> FindPendingPaymentByOrderCodeAsync(long orderCode, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
