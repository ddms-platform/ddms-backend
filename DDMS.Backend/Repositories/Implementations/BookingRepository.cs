using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DDMS.Backend.Repositories.Implementations;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    // Danh sách dùng chung với tầng service để hai bên không bao giờ lệch nhau.
    private static readonly List<string> OccupyingStatuses = BookingStatuses.OccupyingStatuses;

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

    public Task<tour_schedule?> FindScheduleWithCabinsAsync(Guid scheduleId, CancellationToken ct) =>
        _db.tour_schedules
            .Include(s => s.boat).ThenInclude(b => b!.boat_cabins)
            .FirstOrDefaultAsync(s => s.id == scheduleId, ct);

    public async Task<Dictionary<Guid, int>> GetBookedCabinQuantitiesAsync(Guid scheduleId, CancellationToken ct) =>
        await _db.booking_cabins
            .Where(bc => bc.booking.schedule_id == scheduleId && OccupyingStatuses.Contains(bc.booking.status))
            .GroupBy(bc => bc.cabin_id)
            .Select(g => new { CabinId = g.Key, Quantity = g.Sum(x => x.quantity) })
            .ToDictionaryAsync(x => x.CabinId, x => x.Quantity, ct);

    /// <summary>
    /// Chuyến đã kết thúc thì đơn đã trả tiền chuyển sang "đã hoàn thành".
    /// Bulk update, không nạp entity — chạy mỗi giờ trên toàn bảng.
    /// </summary>
    public Task<int> CompleteFinishedToursAsync(DateTime now, CancellationToken ct) =>
        _db.bookings
            .Where(b => BookingStatuses.CompletableStatuses.Contains(b.status)
                && b.schedule.end_time <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.status, BookingStatuses.Completed)
                .SetProperty(b => b.updated_at, now), ct);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) =>
        await _db.Database.BeginTransactionAsync(ct);

    /// <summary>
    /// FOR UPDATE giữ dòng lịch trình tới hết giao dịch, nên request thứ hai phải
    /// chờ request thứ nhất ghi xong rồi mới đếm — không còn cửa sổ để cả hai
    /// cùng thấy "vẫn còn chỗ".
    /// </summary>
    public async Task LockScheduleAsync(Guid scheduleId, CancellationToken ct)
    {
        // ToListAsync trần, KHÔNG thêm FirstOrDefault/Where: hễ gắn thêm toán tử LINQ là
        // EF bọc câu này vào subquery, mà FOR UPDATE trong bảng dẫn xuất thì MySQL
        // vẫn chạy nhưng không còn giữ khoá như mong đợi — khoá hỏng mà im lặng.
        _ = await _db.tour_schedules
            .FromSql($"SELECT * FROM tour_schedules WHERE id = {scheduleId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> GetBookedSeatsAsync(Guid scheduleId, CancellationToken ct) =>
        await _db.bookings
            .Where(b => b.schedule_id == scheduleId && OccupyingStatuses.Contains(b.status))
            .SumAsync(b => b.num_people, ct);

    // Lọc theo boat_id để khách không thể mượn id cabin của tàu khác lấy giá rẻ hơn.
    public async Task<Dictionary<Guid, decimal>> GetCabinPricesAsync(
        Guid boatId, IReadOnlyCollection<Guid> cabinIds, CancellationToken ct) =>
        await _db.boat_cabins
            .Where(c => c.boat_id == boatId && cabinIds.Contains(c.id))
            .Select(c => new { c.id, c.price })
            .ToDictionaryAsync(x => x.id, x => x.price, ct);

    public async Task<Dictionary<Guid, decimal>> GetServicePricesAsync(
        Guid boatId, IReadOnlyCollection<Guid> serviceIds, CancellationToken ct) =>
        await _db.boat_services
            .Where(s => s.boat_id == boatId && serviceIds.Contains(s.id))
            .Select(s => new { s.id, s.price })
            .ToDictionaryAsync(x => x.id, x => x.price, ct);

    public Task<bool> HasActiveBookingForTourDateAsync(
        Guid userId,
        Guid tourId,
        DateTime startOfDay,
        DateTime endOfDay,
        CancellationToken ct) =>
        _db.bookings.AnyAsync(
            b =>
                b.user_id == userId &&
                b.schedule.tour_id == tourId &&
                b.schedule.start_time >= startOfDay &&
                b.schedule.start_time < endOfDay &&
                OccupyingStatuses.Contains(b.status),
            ct);

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

    public Task<booking?> FindBookingWithDetailsAsync(Guid id, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .FirstOrDefaultAsync(b => b.id == id, ct);

    public Task<booking?> FindUserBookingWithLinesAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.booking_cabins)
            .Include(b => b.booking_services)
            .FirstOrDefaultAsync(b => b.id == id && b.user_id == userId, ct);

    public Task<booking?> FindBookingForCheckInByIdAsync(Guid id, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .FirstOrDefaultAsync(b => b.id == id, ct);

    public Task<booking?> FindBookingForCheckInByCodeAsync(string codePrefix, CancellationToken ct)
    {
        var prefix = codePrefix.Trim();
        if (prefix.Length > 8)
            prefix = prefix[..8];

        // Booking code = first 8 chars of GUID string. Use SQL LIKE (translatable by Pomelo).
        prefix = prefix.ToLowerInvariant();
        var pattern = $"{prefix}%";

        return _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .Where(b => EF.Functions.Like(b.id.ToString(), pattern))
            .OrderByDescending(b => b.created_at)
            .FirstOrDefaultAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
