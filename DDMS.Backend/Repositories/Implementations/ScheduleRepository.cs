using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class ScheduleRepository : IScheduleRepository
{
    private readonly AppDbContext _dbContext;

    public ScheduleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<tour_schedule> items, int total)> GetPagedAsync(Guid userId, ScheduleListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var schedulesQuery = _dbContext.tour_schedules
            .Include(x => x.tour)
            .Include(x => x.boat)
            .Include(x => x.dock)
            .Where(x => x.tour.created_by == userId)
            .AsQueryable();

        if (query.tourId.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(x => x.tour_id == query.tourId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            if (TourConstants.ScheduleStatuses.Allowed.Contains(normalizedStatus))
            {
                schedulesQuery = schedulesQuery.Where(x => x.status.ToLower() == normalizedStatus);
            }
        }

        var total = await schedulesQuery.CountAsync();
        var items = await schedulesQuery
            .OrderByDescending(x => x.start_time)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<tour_schedule?> GetByIdAsync(Guid id, Guid userId)
    {
        return _dbContext.tour_schedules
            .Include(x => x.tour)
            .Include(x => x.boat)
            .Include(x => x.dock)
            .FirstOrDefaultAsync(x => x.id == id && x.tour.created_by == userId);
    }

    public Task<bool> BoatExistsAsync(Guid boatId)
    {
        return _dbContext.boats.AnyAsync(x => x.id == boatId);
    }

    public Task<bool> DockExistsAsync(Guid dockId)
    {
        return _dbContext.docks.AnyAsync(x => x.id == dockId);
    }

    public Task<bool> HasBoatTimeOverlapAsync(Guid boatId, DateTime startTime, DateTime endTime, Guid? excludeScheduleId = null)
    {
        return HasResourceTimeOverlapAsync(boatId, null, startTime, endTime, excludeScheduleId);
    }

    public Task<bool> HasDockTimeOverlapAsync(Guid dockId, DateTime startTime, DateTime endTime, Guid? excludeScheduleId = null)
    {
        return HasResourceTimeOverlapAsync(null, dockId, startTime, endTime, excludeScheduleId);
    }

    private Task<bool> HasResourceTimeOverlapAsync(
        Guid? boatId,
        Guid? dockId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeScheduleId)
    {
        var activeStatuses = new List<string>
        {
            TourConstants.ScheduleStatuses.Scheduled,
            TourConstants.ScheduleStatuses.Ongoing
        };

        var query = _dbContext.tour_schedules
            .Where(s => activeStatuses.Contains(s.status))
            .Where(s => s.start_time < endTime && s.end_time > startTime);

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(s => s.id != excludeScheduleId.Value);
        }

        if (boatId.HasValue)
        {
            query = query.Where(s => s.boat_id == boatId.Value);
        }

        if (dockId.HasValue)
        {
            query = query.Where(s => s.dock_id == dockId.Value);
        }

        return query.AnyAsync();
    }

    public async Task AddAsync(tour_schedule entity)
    {
        _dbContext.tour_schedules.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(tour_schedule entity)
    {
        _dbContext.tour_schedules.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<booking>> GetActiveBookingsForScheduleAsync(Guid scheduleId, CancellationToken ct)
    {
        return _dbContext.bookings
            .Include(b => b.user)
            .Where(b => b.schedule_id == scheduleId && b.status != "cancelled")
            .ToListAsync(ct);
    }
}
