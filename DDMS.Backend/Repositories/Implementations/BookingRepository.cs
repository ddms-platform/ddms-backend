using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db) => _db = db;

    public Task<tour_schedule?> FindScheduleWithTourAsync(Guid scheduleId, CancellationToken ct) =>
        _db.tour_schedules
            .Include(s => s.tour)
            .Include(s => s.boat)
            .FirstOrDefaultAsync(s => s.id == scheduleId, ct);

    public Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken ct) =>
        _db.user_roles.AnyAsync(ur => ur.user_id == userId && ur.role.name == roleName, ct);

    // Bulk update: huỷ mọi booking đang giữ chỗ đã quá hạn (atomic, không load entity).
    public Task<int> CancelExpiredHoldsAsync(DateTime now, string reason, CancellationToken ct) =>
        _db.bookings
            .Where(b => b.status == BookingStatuses.Holding
                && b.hold_expired_at != null
                && b.hold_expired_at <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.status, BookingStatuses.Cancelled)
                .SetProperty(b => b.cancel_reason, reason)
                .SetProperty(b => b.cancelled_at, now)
                .SetProperty(b => b.updated_at, now), ct);

    // B2B holds sắp hết hạn, chưa gửi nhắc. Load user + tour để soạn email.
    public Task<List<booking>> GetHoldsNeedingReminderAsync(DateTime now, DateTime remindBefore, string agentRole, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Where(b => b.status == BookingStatuses.Holding
                && !b.hold_reminder_sent
                && b.hold_expired_at != null
                && b.hold_expired_at > now
                && b.hold_expired_at <= remindBefore
                && _db.user_roles.Any(ur => ur.user_id == b.user_id && ur.role.name == agentRole))
            .ToListAsync(ct);

    public void AddBooking(booking entity) => _db.bookings.Add(entity);
    public void AddBookingCabin(booking_cabin entity) => _db.booking_cabins.Add(entity);
    public void AddBookingService(booking_service entity) => _db.booking_services.Add(entity);

    public Task<List<booking>> GetUserBookingsAsync(Guid userId, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.schedule).ThenInclude(s => s.tour).ThenInclude(t => t.tour_images)
            .Where(b => b.user_id == userId)
            .OrderByDescending(b => b.created_at)
            .ToListAsync(ct);

    public Task<booking?> FindUserBookingAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.bookings.FirstOrDefaultAsync(b => b.id == id && b.user_id == userId, ct);

    public Task<booking?> FindUserBookingWithScheduleAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.bookings.Include(b => b.schedule).FirstOrDefaultAsync(b => b.id == id && b.user_id == userId, ct);

    public Task<booking?> FindUserBookingWithDetailsAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .FirstOrDefaultAsync(b => b.id == id && b.user_id == userId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
