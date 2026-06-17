using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class DockRepository : IDockRepository
{
    private readonly AppDbContext _dbContext;

    public DockRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<dock> items, int total)> GetPagedAsync(DockListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var docksQuery = _dbContext.docks
            .Include(d => d.dock_schedules).ThenInclude(s => s.boat)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.search))
        {
            var keyword = query.search.Trim().ToLowerInvariant();
            docksQuery = docksQuery.Where(d =>
                d.name.ToLower().Contains(keyword) ||
                (d.location != null && d.location.ToLower().Contains(keyword)));
        }

        var total = await docksQuery.CountAsync();
        var items = await docksQuery
            .OrderByDescending(d => d.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<dock>> GetAllAsync()
    {
        return await _dbContext.docks
            .Include(d => d.dock_schedules).ThenInclude(s => s.boat)
            .OrderBy(d => d.name)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<dock?> GetByIdAsync(Guid id)
    {
        return _dbContext.docks
            .Include(d => d.dock_schedules).ThenInclude(s => s.boat)
            .FirstOrDefaultAsync(d => d.id == id);
    }

    public async Task<DockStatsResponse> GetStatsAsync()
    {
        var total = await _dbContext.docks.CountAsync();
        var totalMaxBoats = total == 0 ? 0 : await _dbContext.docks.SumAsync(d => d.max_boats);

        return new DockStatsResponse
        {
            total = total,
            totalMaxBoats = totalMaxBoats,
        };
    }

    public async Task<dock> CreateAsync(dock entity)
    {
        _dbContext.docks.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(dock entity)
    {
        _dbContext.docks.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(dock entity)
    {
        _dbContext.docks.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    // ── Schedules ─────────────────────────────────────────────

    public Task<List<dock_schedule>> GetSchedulesAsync(Guid dockId)
    {
        return _dbContext.dock_schedules
            .Include(s => s.boat)
            .Where(s => s.dock_id == dockId)
            .OrderBy(s => s.start_time)
            .ToListAsync();
    }

    public Task<dock_schedule?> GetScheduleAsync(Guid dockId, Guid scheduleId)
    {
        return _dbContext.dock_schedules
            .Include(s => s.boat).ThenInclude(b => b.owner)
            .FirstOrDefaultAsync(s => s.dock_id == dockId && s.id == scheduleId);
    }

    public async Task<dock_schedule> AddScheduleAsync(dock_schedule entity)
    {
        _dbContext.dock_schedules.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteScheduleAsync(dock_schedule entity)
    {
        _dbContext.dock_schedules.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public Task<bool> HasConflictAsync(Guid dockId, Guid boatId, DateTime startTime, DateTime endTime, Guid? excludeId = null)
    {
        return _dbContext.dock_schedules.AnyAsync(s =>
            s.dock_id == dockId &&
            s.boat_id == boatId &&
            (excludeId == null || s.id != excludeId) &&
            s.start_time < endTime &&
            s.end_time > startTime);
    }

    public Task<int> CountActiveSchedulesAsync(Guid dockId, DateTime at)
    {
        return _dbContext.dock_schedules.CountAsync(s =>
            s.dock_id == dockId &&
            s.start_time <= at &&
            s.end_time > at);
    }
}
