using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class TourImageRepository : ITourImageRepository
{
    private readonly AppDbContext _dbContext;

    public TourImageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<tour_image>> GetByTourIdAsync(Guid tourId)
    {
        return _dbContext.tour_images
            .Where(x => x.tour_id == tourId)
            .OrderBy(x => x.sort_order)
            .ThenBy(x => x.created_at)
            .ToListAsync();
    }

    public Task<tour_image?> GetByIdAsync(Guid id, Guid tourId)
    {
        return _dbContext.tour_images
            .FirstOrDefaultAsync(x => x.id == id && x.tour_id == tourId);
    }

    public async Task AddAsync(tour_image entity)
    {
        _dbContext.tour_images.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(tour_image entity)
    {
        _dbContext.tour_images.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(tour_image entity)
    {
        _dbContext.tour_images.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
