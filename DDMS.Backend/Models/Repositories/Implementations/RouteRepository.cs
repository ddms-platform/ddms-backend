using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class RouteRepository : IRouteRepository
{
    private readonly AppDbContext _dbContext;

    public RouteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<route> items, int total)> GetPagedAsync(Guid userId, RouteListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var routesQuery = _dbContext.routes
            .Include(x => x.tour)
            .Where(x => x.tour.created_by == userId)
            .AsQueryable();

        if (query.tourId.HasValue)
        {
            routesQuery = routesQuery.Where(x => x.tour_id == query.tourId.Value);
        }

        var total = await routesQuery.CountAsync();
        var items = await routesQuery
            .OrderBy(x => x.sort_order)
            .ThenBy(x => x.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<route?> GetByIdAsync(Guid id, Guid userId)
    {
        return _dbContext.routes
            .Include(x => x.tour)
            .FirstOrDefaultAsync(x => x.id == id && x.tour.created_by == userId);
    }

    public async Task AddAsync(route entity)
    {
        _dbContext.routes.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(route entity)
    {
        _dbContext.routes.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(route entity)
    {
        _dbContext.routes.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
