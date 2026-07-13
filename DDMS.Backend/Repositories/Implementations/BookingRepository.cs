using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    private static readonly string[] OccupyingStatuses = ["pending", "confirmed", "paid"];

    public BookingRepository(AppDbContext db) => _db = db;

    public Task<tour_schedule?> FindScheduleWithTourAsync(Guid scheduleId, CancellationToken ct) =>
        _db.tour_schedules
            .Include(s => s.tour)
            .Include(s => s.boat)
            .FirstOrDefaultAsync(s => s.id == scheduleId, ct);

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

    public Task<booking?> FindBookingForCheckInByIdAsync(Guid id, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .FirstOrDefaultAsync(b => b.id == id, ct);

    public Task<booking?> FindBookingForCheckInByCodeAsync(string codePrefix, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .Where(b => b.id.ToString().StartsWith(codePrefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => b.created_at)
            .FirstOrDefaultAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
