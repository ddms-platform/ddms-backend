using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly AppDbContext _db;
    public AdminDashboardRepository(AppDbContext db) => _db = db;

    public Task<int> CountUsersAsync(CancellationToken ct) => _db.users.CountAsync(ct);
    public Task<int> CountUsersBeforeAsync(DateTime before, CancellationToken ct) =>
        _db.users.CountAsync(u => u.created_at < before, ct);

    public Task<int> CountToursByStatusAsync(IReadOnlyCollection<string> statuses, CancellationToken ct) =>
        _db.tours.CountAsync(t => statuses.Contains(t.status), ct);
    public Task<int> CountAllToursAsync(CancellationToken ct) => _db.tours.CountAsync(ct);
    public Task<int> CountToursBeforeAsync(DateTime before, CancellationToken ct) =>
        _db.tours.CountAsync(t => t.created_at < before, ct);

    public Task<int> CountBoatsByStatusAsync(IReadOnlyCollection<string> statuses, CancellationToken ct) =>
        _db.boats.CountAsync(b => statuses.Contains(b.status) && !b.is_deleted, ct);
    public Task<int> CountActiveBoatsAsync(CancellationToken ct) =>
        _db.boats.CountAsync(b => !b.is_deleted, ct);
    public Task<int> CountActiveBoatsBeforeAsync(DateTime before, CancellationToken ct) =>
        _db.boats.CountAsync(b => !b.is_deleted && b.created_at < before, ct);

    public async Task<decimal> SumRevenueAsync(IReadOnlyCollection<string> statuses, DateTime from, DateTime? toExclusive, CancellationToken ct)
    {
        var q = _db.bookings.Where(b => statuses.Contains(b.status) && b.created_at >= from);
        if (toExclusive.HasValue) q = q.Where(b => b.created_at < toExclusive.Value);
        return await q.SumAsync(b => (decimal?)b.total_price, ct) ?? 0m;
    }

    public Task<int> CountBookingsByStatusAsync(IReadOnlyCollection<string> statuses, CancellationToken ct) =>
        _db.bookings.CountAsync(b => statuses.Contains(b.status), ct);

    public Task<List<booking>> GetRecentBookingsAsync(int take, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .OrderByDescending(b => b.created_at)
            .Take(take)
            .ToListAsync(ct);

    public Task<List<owner_profile>> GetPendingOwnerProfilesAsync(IReadOnlyCollection<string> statuses, int take, CancellationToken ct) =>
        _db.owner_profiles
            .Include(op => op.user)
            .Where(op => statuses.Contains(op.status))
            .OrderByDescending(op => op.created_at)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> CountDocksAsync(CancellationToken ct) => _db.docks.CountAsync(ct);

    public Task<int> CountActivePromotionsAsync(DateTime now, CancellationToken ct) =>
        _db.promotions.CountAsync(p => p.is_active == true && p.valid_from <= now
            && (p.valid_until == null || p.valid_until >= now), ct);

    public Task<int> CountAuditLogsSinceAsync(DateTime since, CancellationToken ct) =>
        _db.audit_logs.CountAsync(al => al.created_at >= since, ct);

    public Task<List<tour>> GetAllToursWithCreatorAsync(CancellationToken ct) =>
        _db.tours.Include(t => t.created_byNavigation).ToListAsync(ct);

    public Task<int> CountCompletedBookingsForTourAsync(Guid tourId, CancellationToken ct) =>
        _db.bookings.CountAsync(b => b.schedule.tour_id == tourId && b.status == "completed", ct);

    public async Task<decimal> SumCompletedRevenueForTourAsync(Guid tourId, CancellationToken ct) =>
        await _db.bookings
            .Where(b => b.schedule.tour_id == tourId && b.status == "completed")
            .SumAsync(b => (decimal?)b.total_price, ct) ?? 0m;

    public Task<owner_profile?> FindOwnerProfileByUserAsync(Guid userId, CancellationToken ct) =>
        _db.owner_profiles.FirstOrDefaultAsync(op => op.user_id == userId, ct);
}
