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

    public async Task<List<TourStatsItem>> GetTourStatsAsync(Guid ownerId, CancellationToken ct)
    {
        var statuses = _billing.RevenueRelevantBookingStatuses.ToList();
        
        var bookings = await _db.bookings
            .Where(b => b.schedule.boat != null && b.schedule.boat.owner_id == ownerId && statuses.Contains(b.status))
            .Select(b => new { TourName = b.schedule.tour.name, b.total_price })
            .ToListAsync(ct);
            
        return bookings.GroupBy(b => b.TourName)
            .Select(g => new TourStatsItem
            {
                TourName = g.Key,
                BookingsCount = g.Count(),
                TotalRevenue = g.Sum(x => x.total_price)
            })
            .ToList();
    }

    public async Task<List<ScheduleListItem>> GetSchedulesAsync(Guid ownerId, int month, int year, CancellationToken ct)
    {
        var schedules = await _db.tour_schedules
            .Where(ts => ts.boat != null && ts.boat.owner_id == ownerId
                      && ts.start_time.Year == year && ts.start_time.Month == month)
            .Select(ts => new 
            {
                ts.id,
                ts.tour_id,
                TourName = ts.tour.name,
                TourStatus = ts.tour.status,
                BoatName = ts.boat!.name,
                ts.boat_id,
                ts.start_time,
                ts.end_time,
                ts.status
            })
            .ToListAsync(ct);

        return schedules.Select(ts => new ScheduleListItem
        {
            Id = ts.id,
            TourId = ts.tour_id,
            TourName = ts.TourName,
            TourStatus = ts.TourStatus,
            BoatName = ts.BoatName ?? "N/A",
            BoatId = ts.boat_id ?? Guid.Empty,
            StartTime = ts.start_time,
            EndTime = ts.end_time,
            Status = ts.status
        }).ToList();
    }

    /// <summary>
    /// Tour của chủ thuyền để quản lý trên dashboard.
    /// - Tour bị từ chối: luôn hiện (kể cả khi đã xoá thuyền) để xem lý do Admin.
    /// - Còn lại: chỉ hiện khi còn gắn thuyền chưa xoá (lịch / cabin / combo).
    /// </summary>
    public async Task<List<OwnerTourListItem>> GetOwnerToursAsync(Guid ownerId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rejected = TourConstants.Statuses.Rejected;

        var tours = await _db.tours
            .Where(t =>
                (t.created_by == ownerId && t.status == rejected)
                || t.tour_schedules.Any(ts => ts.boat != null && ts.boat.owner_id == ownerId)
                || _db.boat_cabins.Any(c => c.tour_id == t.id && c.boat.owner_id == ownerId)
                || _db.boat_services.Any(s => s.tour_id == t.id && s.boat.owner_id == ownerId))
            .Select(t => new
            {
                t.id,
                t.name,
                t.status,
                t.price,
                t.duration_minutes,
                t.location,
                t.service_type,
                t.created_at,
                t.rejection_reason,
                ThumbnailUrl = t.tour_images
                    .OrderBy(i => i.sort_order)
                    .Select(i => i.image_url)
                    .FirstOrDefault(),
                ScheduleBoats = t.tour_schedules
                    .Where(ts => ts.boat != null && ts.boat.owner_id == ownerId)
                    .Select(ts => new { ts.boat!.id, ts.boat.name })
                    .ToList(),
                ScheduleCount = t.tour_schedules.Count(),
                UpcomingScheduleCount = t.tour_schedules.Count(ts => ts.start_time >= now),
                NextScheduleAt = t.tour_schedules
                    .Where(ts => ts.start_time >= now)
                    .OrderBy(ts => ts.start_time)
                    .Select(ts => (DateTime?)ts.start_time)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var tourIds = tours.Select(t => t.id).ToList();
        var cabinBoats = await _db.boat_cabins
            .Where(c => c.tour_id != null && tourIds.Contains(c.tour_id.Value) && c.boat.owner_id == ownerId)
            .Select(c => new { TourId = c.tour_id!.Value, c.boat.id, c.boat.name })
            .ToListAsync(ct);
        var serviceBoats = await _db.boat_services
            .Where(s => s.tour_id != null && tourIds.Contains(s.tour_id.Value) && s.boat.owner_id == ownerId)
            .Select(s => new { TourId = s.tour_id!.Value, s.boat.id, s.boat.name })
            .ToListAsync(ct);

        return tours
            .OrderByDescending(t => t.created_at)
            .Select(t =>
            {
                var boats = t.ScheduleBoats
                    .Select(b => (b.id, b.name))
                    .Concat(cabinBoats.Where(b => b.TourId == t.id).Select(b => (b.id, b.name)))
                    .Concat(serviceBoats.Where(b => b.TourId == t.id).Select(b => (b.id, b.name)))
                    .DistinctBy(b => b.id)
                    .ToList();

                return new OwnerTourListItem
                {
                    Id = t.id,
                    Name = t.name,
                    Status = t.status,
                    Price = t.price,
                    DurationMinutes = t.duration_minutes,
                    Location = t.location,
                    ServiceType = t.service_type,
                    ThumbnailUrl = t.ThumbnailUrl,
                    BoatNames = boats.Select(b => b.name).ToList(),
                    BoatIds = boats.Select(b => b.id).ToList(),
                    PrimaryBoatId = boats.Count > 0 ? boats[0].id : null,
                    ScheduleCount = t.ScheduleCount,
                    UpcomingScheduleCount = t.UpcomingScheduleCount,
                    NextScheduleAt = t.NextScheduleAt,
                    CreatedAt = t.created_at,
                    RejectionReason = t.rejection_reason
                };
            })
            .ToList();
    }

    public async Task<List<RecentBookingItem>> GetRecentBookingsAsync(Guid ownerId, int take, CancellationToken ct)
    {
        var bookings = await _db.bookings
            .Where(b => b.schedule.boat != null && b.schedule.boat.owner_id == ownerId)
            .OrderByDescending(b => b.created_at)
            .Take(take)
            .Select(b => new 
            {
                Id = b.id,
                CustomerName = b.user.full_name,
                ServiceName = b.schedule.tour.name,
                BoatName = b.schedule.boat!.name,
                Time = b.schedule.start_time,
                Value = b.total_price,
                Status = b.status
            })
            .ToListAsync(ct);

        return bookings.Select(b => new RecentBookingItem
        {
            Id = b.Id,
            BookingId = b.Id.ToString().Substring(0, 8).ToUpper(),
            CustomerName = b.CustomerName,
            ServiceName = b.ServiceName,
            BoatName = b.BoatName ?? "N/A",
            Time = b.Time,
            Value = b.Value,
            Status = b.Status
        }).ToList();
    }

    /// <summary>
    /// Tour Live gắn thuyền — dùng cho dropdown tạo lịch.
    /// Phải lấy từ cabin/combo chứ không chỉ từ tour_schedules: tour mới duyệt
    /// thường chưa có lịch, lọc theo lịch sẽ làm dropdown trống (gà-trứng).
    /// </summary>
    public async Task<List<OwnerBoatResource>> GetOwnerResourcesAsync(Guid ownerId, CancellationToken ct)
    {
        var boats = await _db.boats
            .Where(b => b.owner_id == ownerId)
            .Select(b => new { b.id, b.name })
            .ToListAsync(ct);

        if (boats.Count == 0)
            return new List<OwnerBoatResource>();

        var boatIds = boats.Select(b => b.id).ToList();
        var linked = await GetActiveTourBoatLinksAsync(boatIds, ct);

        return boats.Select(b => new OwnerBoatResource
        {
            Id = b.id,
            Name = b.name,
            Tours = linked
                .Where(t => t.BoatId == b.id)
                .Select(t => new OwnerTourResource { Id = t.TourId, Name = t.Name })
                .DistinctBy(t => t.Id)
                .ToList()
        }).ToList();
    }

    /// <summary>
    /// Không join Guid? trực tiếp (Pomelo/MySQL dễ trả 0 dòng). Lấy id rồi
    /// Contains — cùng kiểu với GetOwnerToursAsync.
    /// </summary>
    private async Task<List<(Guid BoatId, Guid TourId, string Name)>> GetActiveTourBoatLinksAsync(
        List<Guid> boatIds, CancellationToken ct)
    {
        var fromSchedules = await _db.tour_schedules
            .Where(ts =>
                ts.boat_id != null
                && boatIds.Contains(ts.boat_id.Value)
                && ts.tour.status == TourStatuses.Active)
            .Select(ts => new { BoatId = ts.boat_id!.Value, TourId = ts.tour_id, Name = ts.tour.name })
            .ToListAsync(ct);

        var fromCabins = await _db.boat_cabins
            .Where(c => c.tour_id != null && boatIds.Contains(c.boat_id))
            .Select(c => new { BoatId = c.boat_id, TourId = c.tour_id!.Value })
            .ToListAsync(ct);

        var fromCombos = await _db.boat_services
            .Where(s => s.tour_id != null && boatIds.Contains(s.boat_id))
            .Select(s => new { BoatId = s.boat_id, TourId = s.tour_id!.Value })
            .ToListAsync(ct);

        var cabinComboIds = fromCabins.Select(x => x.TourId)
            .Concat(fromCombos.Select(x => x.TourId))
            .Distinct()
            .ToList();

        var activeById = cabinComboIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.tours
                .Where(t => cabinComboIds.Contains(t.id) && t.status == TourStatuses.Active)
                .Select(t => new { t.id, t.name })
                .ToDictionaryAsync(t => t.id, t => t.name, ct);

        var fromCabinLive = fromCabins
            .Where(x => activeById.ContainsKey(x.TourId))
            .Select(x => new { x.BoatId, x.TourId, Name = activeById[x.TourId] });
        var fromComboLive = fromCombos
            .Where(x => activeById.ContainsKey(x.TourId))
            .Select(x => new { x.BoatId, x.TourId, Name = activeById[x.TourId] });

        return fromSchedules
            .Concat(fromCabinLive)
            .Concat(fromComboLive)
            .Select(x => (x.BoatId, x.TourId, x.Name))
            .Distinct()
            .ToList();
    }

    public Task<boat?> FindOwnerBoatAsync(Guid boatId, Guid ownerId, CancellationToken ct) =>
        _db.boats.FirstOrDefaultAsync(b => b.id == boatId && b.owner_id == ownerId, ct);

    public Task<tour?> FindTourAsync(Guid tourId, CancellationToken ct) =>
        _db.tours.FindAsync(new object?[] { tourId }, ct).AsTask();

    public Task<bool> HasTourScheduleOverlapAsync(Guid ownerId, Guid tourId, DateTime start, DateTime end, CancellationToken ct) =>
        _db.tour_schedules.AnyAsync(
            ts =>
                ts.tour_id == tourId &&
                ts.boat != null &&
                ts.boat.owner_id == ownerId &&
                ts.status != "cancelled" &&
                ts.start_time < end &&
                ts.end_time > start,
            ct);

    public Task<bool> HasScheduleOverlapAsync(Guid boatId, DateTime start, DateTime end, CancellationToken ct) =>
        _db.tour_schedules.AnyAsync(
            ts =>
                ts.boat_id == boatId &&
                ts.status != "cancelled" &&
                ts.start_time < end &&
                ts.end_time > start,
            ct);

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
