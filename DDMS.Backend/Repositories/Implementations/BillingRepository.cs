using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BillingRepository : IBillingRepository
{
    private readonly AppDbContext _db;

    public BillingRepository(AppDbContext db) => _db = db;

    public Task<List<booking>> GetOwnerRevenueBookingsAsync(Guid ownerId, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.user)
            .Where(b => b.schedule.tour.created_by == ownerId
                     && BillingRates.RevenueRelevantBookingStatuses.Contains(b.status))
            .OrderByDescending(b => b.created_at)
            .ToListAsync(ct);

    public Task<List<boat_maintenance>> GetOwnerApprovedMaintenancesAsync(Guid ownerId, CancellationToken ct) =>
        _db.boat_maintenances
            .Include(m => m.boat)
            .Include(m => m.port_maintenance_service)
            .Where(m => m.boat.owner_id == ownerId && m.status == "approved" && !m.is_deleted)
            .OrderByDescending(m => m.start_time)
            .ToListAsync(ct);

    public Task<List<boat>> GetOwnerBoatsAsync(Guid ownerId, CancellationToken ct) =>
        _db.boats.Where(b => b.owner_id == ownerId && !b.is_deleted).ToListAsync(ct);

    public Task<List<dock_schedule>> GetSchedulesForBoatsAsync(IReadOnlyCollection<Guid> boatIds, CancellationToken ct) =>
        boatIds.Count == 0
            ? Task.FromResult(new List<dock_schedule>())
            : _db.dock_schedules.Where(ds => boatIds.Contains(ds.boat_id)).ToListAsync(ct);

    public Task<List<owner_payment>> GetOwnerPaymentsAsync(Guid ownerId, CancellationToken ct) =>
        _db.owner_payments
            .Where(p => p.owner_id == ownerId)
            .OrderByDescending(p => p.created_at)
            .ToListAsync(ct);

    public Task<decimal> GetOwnerTotalPaidAsync(Guid ownerId, CancellationToken ct) =>
        _db.owner_payments
            .Where(p => p.owner_id == ownerId && p.status == "paid")
            .SumAsync(p => p.amount, ct);

    public Task AddPaymentAsync(owner_payment payment, CancellationToken ct)
    {
        _db.owner_payments.Add(payment);
        return Task.CompletedTask;
    }

    public Task<owner_payment?> FindPendingPaymentByOrderCodeAsync(long orderCode, CancellationToken ct) =>
        _db.owner_payments.FirstOrDefaultAsync(p => p.payos_order_code == orderCode && p.status == "pending", ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
