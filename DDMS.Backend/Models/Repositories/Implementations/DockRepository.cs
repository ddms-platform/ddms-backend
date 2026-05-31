using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

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

        var docksQuery = _dbContext.docks.AsQueryable();

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
            .OrderBy(d => d.name)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<dock?> GetByIdAsync(Guid id)
    {
        return _dbContext.docks
            .FirstOrDefaultAsync(d => d.id == id);
    }

    public async Task<DockStatsResponse> GetStatsAsync()
    {
        var stats = await _dbContext.docks
            .GroupBy(_ => 1)
            .Select(g => new
            {
                total = g.Count(),
                totalMaxBoats = g.Sum(d => d.max_boats),
            })
            .FirstOrDefaultAsync();

        if (stats is null)
            return new DockStatsResponse();

        return new DockStatsResponse
        {
            total = stats.total,
            totalMaxBoats = stats.totalMaxBoats,
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
}
