using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BoatRepository : IBoatRepository
{
    private readonly AppDbContext _dbContext;

    public BoatRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<boat> items, int total)> GetPagedAsync(BoatListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var boatsQuery = _dbContext.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.boat_images)
            .Include(b => b.tour_schedules)
            .Include(b => b.boat_maintenances)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.search))
        {
            var keyword = query.search.Trim().ToLowerInvariant();
            boatsQuery = boatsQuery.Where(b =>
                b.name.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var statusVal = query.status.Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;
            if (statusVal == "running")
            {
                boatsQuery = boatsQuery.Where(b => b.tour_schedules.Any(ts => 
                    ts.start_time <= now && now <= ts.end_time && 
                    (ts.status == "ongoing" || ts.status == "scheduled") &&
                    !b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time)));
            }
            else if (statusVal == "idle")
            {
                boatsQuery = boatsQuery.Where(b => !b.tour_schedules.Any(ts => 
                    ts.start_time <= now && now <= ts.end_time && 
                    (ts.status == "ongoing" || ts.status == "scheduled") &&
                    !b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time)));
            }
            else if (statusVal == "maintenance")
            {
                boatsQuery = boatsQuery.Where(b => b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.type))
        {
            var typeVal = query.type.Trim().ToLowerInvariant();
            boatsQuery = boatsQuery.Where(b => b.type != null && b.type.ToLower() == typeVal);
        }

        var total = await boatsQuery.CountAsync();
        var items = await boatsQuery
            .OrderByDescending(b => b.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<boat>> GetAllAsync()
    {
        return await _dbContext.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.boat_images)
            .Include(b => b.tour_schedules)
            .Include(b => b.boat_maintenances)
            .OrderBy(b => b.name)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<boat?> GetByIdAsync(Guid id)
    {
        return _dbContext.boats
            .FirstOrDefaultAsync(b => b.id == id);
    }

    public Task<boat?> GetByIdWithDetailsAsync(Guid id)
    {
        return _dbContext.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.tour_schedules).ThenInclude(ts => ts.tour)
            .Include(b => b.boat_images.OrderBy(i => i.sort_order))
            .Include(b => b.boat_maintenances.OrderByDescending(m => m.start_time))
                .ThenInclude(m => m.port_maintenance_service)
            .FirstOrDefaultAsync(b => b.id == id);
    }

    public async Task<BoatStatsResponse> GetStatsAsync()
    {
        var boats = await _dbContext.boats
            .Include(b => b.tour_schedules)
            .Include(b => b.boat_maintenances)
            .ToListAsync();
        var now = DateTime.UtcNow;
        var running = boats.Count(b => b.tour_schedules != null && b.tour_schedules.Any(ts =>
            ts.start_time <= now && now <= ts.end_time && (ts.status == "ongoing" || ts.status == "scheduled") &&
            !(b.boat_maintenances != null && b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time))));
            
        var total = boats.Count;
        var idle = total - running;
        var cabins = await _dbContext.boat_cabins.CountAsync();

        return new BoatStatsResponse
        {
            total = total,
            idle = idle,
            running = running,
            totalCabins = cabins
        };
    }

    public async Task<boat> CreateAsync(boat entity)
    {
        _dbContext.boats.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(boat entity)
    {
        _dbContext.boats.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(boat entity)
    {
        entity.is_deleted = true;
        _dbContext.boats.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    // ── Owner-specific ────────────────────────────────────────

    public async Task<(List<boat> items, int total)> GetPagedByOwnerAsync(Guid ownerId, OwnerBoatListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var q = _dbContext.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.boat_images)
            .Include(b => b.tour_schedules)
            .Include(b => b.boat_maintenances)
            .Where(b => b.owner_id == ownerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.search))
        {
            var kw = query.search.Trim().ToLower();
            q = q.Where(b => b.name.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var statusVal = query.status.Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;
            if (statusVal == "running")
            {
                q = q.Where(b => b.tour_schedules.Any(ts => 
                    ts.start_time <= now && now <= ts.end_time && 
                    (ts.status == "ongoing" || ts.status == "scheduled") &&
                    !b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time)));
            }
            else if (statusVal == "idle")
            {
                q = q.Where(b => !b.tour_schedules.Any(ts => 
                    ts.start_time <= now && now <= ts.end_time && 
                    (ts.status == "ongoing" || ts.status == "scheduled") &&
                    !b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time)));
            }
            else if (statusVal == "maintenance")
            {
                q = q.Where(b => b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.type))
            q = q.Where(b => b.type != null && b.type.ToLower() == query.type.Trim().ToLower());

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(b => b.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<boat?> GetByIdAndOwnerAsync(Guid id, Guid ownerId)
    {
        return _dbContext.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.tour_schedules).ThenInclude(ts => ts.tour)
            .Include(b => b.boat_images.OrderBy(i => i.sort_order))
            .Include(b => b.boat_maintenances.OrderByDescending(m => m.start_time))
                .ThenInclude(m => m.port_maintenance_service)
            .FirstOrDefaultAsync(b => b.id == id && b.owner_id == ownerId);
    }

    public async Task<BoatStatsResponse> GetStatsByOwnerAsync(Guid ownerId)
    {
        var boats = await _dbContext.boats
            .Include(b => b.tour_schedules)
            .Include(b => b.boat_maintenances)
            .Where(b => b.owner_id == ownerId)
            .ToListAsync();
            
        var now = DateTime.UtcNow;
        var runningCount = boats.Count(b => b.tour_schedules != null && b.tour_schedules.Any(ts =>
            ts.start_time <= now && now <= ts.end_time && (ts.status == "ongoing" || ts.status == "scheduled") &&
            !(b.boat_maintenances != null && b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time))));
            
        var total = boats.Count;
        var idle = total - runningCount;
        var cabins = await _dbContext.boat_cabins.CountAsync(c => c.boat.owner_id == ownerId);

        var pastYearDate = DateTime.UtcNow.AddMonths(-11);
        pastYearDate = new DateTime(pastYearDate.Year, pastYearDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var bookings = await _dbContext.bookings
            .Include(b => b.schedule)
            .ThenInclude(s => s.boat)
            .Where(b => b.schedule.boat != null && b.schedule.boat.owner_id == ownerId && b.status == "completed" && b.created_at >= pastYearDate)
            .ToListAsync();

        var monthlyProfits = new List<MonthlyProfit>();
        for (int i = 0; i < 12; i++)
        {
            var targetMonth = pastYearDate.AddMonths(i);
            var profit = bookings
                .Where(b => b.created_at.Year == targetMonth.Year && b.created_at.Month == targetMonth.Month)
                .Sum(b => b.total_price);
            
            monthlyProfits.Add(new MonthlyProfit
            {
                Month = targetMonth.ToString("MMM").ToUpper(),
                Year = targetMonth.Year,
                Profit = profit
            });
        }

        return new BoatStatsResponse
        {
            total = total,
            idle = idle,
            running = runningCount,
            totalCabins = cabins,
            monthlyProfits = monthlyProfits
        };
    }
}
