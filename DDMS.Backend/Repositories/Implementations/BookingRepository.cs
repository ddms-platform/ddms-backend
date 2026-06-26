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
        _db.tour_schedules.Include(s => s.tour).FirstOrDefaultAsync(s => s.id == scheduleId, ct);

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
