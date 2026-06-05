using DDMS.Backend.Data;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class TourRepository : ITourRepository
{
    private readonly AppDbContext _dbContext;

    public TourRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<tour> items, int total)> GetPagedAsync(Guid userId, TourListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var toursQuery = _dbContext.tours
            .Where(x => x.created_by == userId);

        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            if (TourConstants.Statuses.Allowed.Contains(normalizedStatus))
            {
                toursQuery = toursQuery.Where(x => x.status.ToLower() == normalizedStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.location))
        {
            var location = query.location.Trim();
            toursQuery = toursQuery.Where(x => x.location != null && x.location.Contains(location));
        }

        var total = await toursQuery.CountAsync();
        var items = await toursQuery
            .OrderByDescending(x => x.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<tour?> GetByIdAsync(Guid id, Guid userId)
    {
        return _dbContext.tours
            .FirstOrDefaultAsync(x => x.id == id && x.created_by == userId);
    }

    public Task<tour?> GetActiveByIdAsync(Guid id)
    {
        return _dbContext.tours
            .FirstOrDefaultAsync(x => x.id == id && x.status == TourConstants.Statuses.Active);
    }

    public async Task AddAsync(tour entity)
    {
        _dbContext.tours.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(tour entity)
    {
        _dbContext.tours.Update(entity);
        await _dbContext.SaveChangesAsync();
    }
}
