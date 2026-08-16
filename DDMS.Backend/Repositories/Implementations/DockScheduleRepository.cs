using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class DockScheduleRepository : IDockScheduleRepository
{
    private readonly AppDbContext _dbContext;

    public DockScheduleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<dock_schedule> items, int total)> GetPagedAsync(DockScheduleListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var schedulesQuery = _dbContext.dock_schedules
            .Include(x => x.dock)
            .Include(x => x.boat)
            .AsQueryable();

        if (query.dockId.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(x => x.dock_id == query.dockId.Value);
        }

        if (query.boatId.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(x => x.boat_id == query.boatId.Value);
        }

        var total = await schedulesQuery.CountAsync();
        var items = await schedulesQuery
            .OrderBy(x => x.start_time)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<dock_schedule?> GetByIdAsync(Guid id)
    {
        return _dbContext.dock_schedules
            .Include(x => x.dock)
            .Include(x => x.boat)
            .FirstOrDefaultAsync(x => x.id == id);
    }

    public Task<dock?> GetDockAsync(Guid dockId)
    {
        return _dbContext.docks.FirstOrDefaultAsync(x => x.id == dockId);
    }

    public Task<boat?> GetBoatAsync(Guid boatId)
    {
        return _dbContext.boats.FirstOrDefaultAsync(x => x.id == boatId);
    }

    public Task<bool> HasBoatOverlapAsync(Guid boatId, DateTime startTime, DateTime endTime, Guid? excludeId = null)
    {
        var query = _dbContext.dock_schedules
            .Where(x => x.boat_id == boatId &&
                        x.start_time < endTime &&
                        x.end_time > startTime);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.id != excludeId.Value);
        }

        return query.AnyAsync();
    }

    public Task<int> CountDockOverlapsAsync(Guid dockId, DateTime startTime, DateTime endTime, Guid? excludeId = null)
    {
        var query = _dbContext.dock_schedules
            .Where(x => x.dock_id == dockId &&
                        x.start_time < endTime &&
                        x.end_time > startTime);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.id != excludeId.Value);
        }

        return query.CountAsync();
    }

    public Task<bool> HasBerthConflictAsync(
        Guid dockId, string berthCode, DateTime startTime, DateTime endTime, Guid? excludeId = null)
    {
        var query = _dbContext.dock_schedules.Where(x =>
            x.dock_id == dockId
            && x.berth_code == berthCode
            // Giao nhau ve thoi gian: bat dau truoc khi cai kia ket thuc, va
            // ket thuc sau khi cai kia bat dau.
            && x.start_time < endTime
            && x.end_time > startTime);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.id != excludeId.Value);
        }

        return query.AnyAsync();
    }

    public Task SaveChangesAsync(CancellationToken ct) => _dbContext.SaveChangesAsync(ct);

    public async Task AddAsync(dock_schedule entity)
    {
        _dbContext.dock_schedules.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(dock_schedule entity)
    {
        _dbContext.dock_schedules.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(dock_schedule entity)
    {
        _dbContext.dock_schedules.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
