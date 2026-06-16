using DDMS.Backend.Common.Constants;
using DDMS.Backend.Configurations;
using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.OwnerToursDashboard;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Repositories.Implementations;

public class OwnerToursDashboardRepository : IOwnerToursDashboardRepository
{
    private readonly AppDbContext _db;
    private readonly BillingOptions _billing;

    public OwnerToursDashboardRepository(AppDbContext db, IOptions<BillingOptions> billing)
    {
        _db = db;
        _billing = billing.Value;
    }

    public Task<List<TourStatsItem>> GetTourStatsAsync(Guid ownerId, CancellationToken ct)
    {
        var statuses = _billing.RevenueRelevantBookingStatuses;
        return _db.tours
            .Select(t => new TourStatsItem
            {
                TourName = t.name,
                BookingsCount = t.tour_schedules
                    .Where(ts => ts.boat != null && ts.boat.owner_id == ownerId)
                    .SelectMany(ts => ts.bookings)
                    .Count(b => statuses.Contains(b.status.ToLower())),
                TotalRevenue = t.tour_schedules
                    .Where(ts => ts.boat != null && ts.boat.owner_id == ownerId)
                    .SelectMany(ts => ts.bookings)
                    .Where(b => statuses.Contains(b.status.ToLower()))
                    .Sum(b => (decimal?)b.total_price) ?? 0m
            })
            .Where(x => x.BookingsCount > 0)
            .ToListAsync(ct);
    }

    public Task<List<ScheduleListItem>> GetSchedulesAsync(Guid ownerId, int month, int year, CancellationToken ct) =>
        _db.tour_schedules
            .Include(ts => ts.tour)
            .Include(ts => ts.boat)
            .Where(ts => ts.boat != null && ts.boat.owner_id == ownerId
                      && ts.start_time.Year == year && ts.start_time.Month == month)
            .Select(ts => new ScheduleListItem
            {
                Id = ts.id,
                TourName = ts.tour.name,
                BoatName = ts.boat != null ? ts.boat.name : "N/A",
                BoatId = ts.boat_id ?? Guid.Empty,
                StartTime = ts.start_time,
                EndTime = ts.end_time,
                Status = ts.status
            })
            .ToListAsync(ct);

    public Task<List<RecentBookingItem>> GetRecentBookingsAsync(Guid ownerId, int take, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .Where(b => b.schedule.boat != null && b.schedule.boat.owner_id == ownerId)
            .OrderByDescending(b => b.created_at)
            .Take(take)
            .Select(b => new RecentBookingItem
            {
                Id = b.id,
                BookingId = b.id.ToString().Substring(0, 8).ToUpper(),
                CustomerName = b.user.full_name,
                ServiceName = b.schedule.tour.name,
                BoatName = b.schedule.boat != null ? b.schedule.boat.name : "N/A",
                Time = b.schedule.start_time,
                Value = b.total_price,
                Status = b.status
            })
            .ToListAsync(ct);

    public Task<List<OwnerBoatResource>> GetOwnerResourcesAsync(Guid ownerId, CancellationToken ct) =>
        _db.boats
            .Include(b => b.tour_schedules).ThenInclude(ts => ts.tour)
            .Where(b => b.owner_id == ownerId)
            .Select(b => new OwnerBoatResource
            {
                Id = b.id,
                Name = b.name,
                Tours = b.tour_schedules
                    .Where(ts => ts.tour.status == TourStatuses.Active)
                    .Select(ts => new OwnerTourResource { Id = ts.tour_id, Name = ts.tour.name })
                    .Distinct()
                    .ToList()
            })
            .ToListAsync(ct);

    public Task<boat?> FindOwnerBoatAsync(Guid boatId, Guid ownerId, CancellationToken ct) =>
        _db.boats.FirstOrDefaultAsync(b => b.id == boatId && b.owner_id == ownerId, ct);

    public Task<tour?> FindTourAsync(Guid tourId, CancellationToken ct) =>
        _db.tours.FindAsync(new object?[] { tourId }, ct).AsTask();

    public Task<bool> HasScheduleOverlapAsync(Guid boatId, DateTime start, DateTime end, CancellationToken ct) =>
        _db.tour_schedules.AnyAsync(ts => ts.boat_id == boatId && ts.start_time < end && ts.end_time > start, ct);

    public void AddSchedule(tour_schedule schedule) => _db.tour_schedules.Add(schedule);

    public Task<booking?> FindOwnerBookingWithDetailsAsync(Guid bookingId, Guid ownerId, CancellationToken ct) =>
        _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule).ThenInclude(s => s.tour)
            .Include(b => b.schedule).ThenInclude(s => s.boat)
            .FirstOrDefaultAsync(b => b.id == bookingId && b.schedule.boat != null && b.schedule.boat.owner_id == ownerId, ct);

    public Task<List<tour_schedule>> GetSchedulesByDayMonthAsync(int day, int month, CancellationToken ct) =>
        _db.tour_schedules.Where(ts => ts.start_time.Day == day && ts.start_time.Month == month).ToListAsync(ct);

    public void RemoveSchedules(IEnumerable<tour_schedule> schedules) => _db.tour_schedules.RemoveRange(schedules);

    public Task<List<boat>> GetAllBoatsOrderedAsync(CancellationToken ct) =>
        _db.boats.OrderBy(b => b.created_at).ToListAsync(ct);

    public Task<boat?> GetFirstBoatAsync(CancellationToken ct) => _db.boats.FirstOrDefaultAsync(ct);

    public void AddTours(IEnumerable<tour> tours) => _db.tours.AddRange(tours);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
