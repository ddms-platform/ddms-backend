using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

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
            boatsQuery = boatsQuery.Where(b => b.status.ToLower() == statusVal);
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
            .Include(b => b.boat_images.OrderBy(i => i.sort_order))
            .Include(b => b.boat_maintenances.OrderByDescending(m => m.start_time))
            .FirstOrDefaultAsync(b => b.id == id);
    }

    public async Task<BoatStatsResponse> GetStatsAsync()
    {
        var total = await _dbContext.boats.CountAsync();
        var idle = await _dbContext.boats.CountAsync(b => b.status == "idle");
        var running = await _dbContext.boats.CountAsync(b => b.status == "running");

        return new BoatStatsResponse
        {
            total = total,
            idle = idle,
            running = running,
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
        _dbContext.boats.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
