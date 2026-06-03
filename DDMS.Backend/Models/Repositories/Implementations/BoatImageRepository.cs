using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class BoatImageRepository : IBoatImageRepository
{
    private readonly AppDbContext _dbContext;

    public BoatImageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<boat_image>> GetByBoatIdAsync(Guid boatId)
    {
        return await _dbContext.boat_images
            .Where(i => i.boat_id == boatId)
            .OrderBy(i => i.sort_order)
            .ToListAsync();
    }

    public Task<boat_image?> GetByIdAsync(Guid id)
    {
        return _dbContext.boat_images
            .FirstOrDefaultAsync(i => i.id == id);
    }

    public async Task<boat_image> CreateAsync(boat_image entity)
    {
        _dbContext.boat_images.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(boat_image entity)
    {
        _dbContext.boat_images.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(List<(Guid id, int sortOrder)> updates)
    {
        foreach (var (id, sortOrder) in updates)
        {
            var img = await _dbContext.boat_images.FindAsync(id);
            if (img is not null)
            {
                img.sort_order = sortOrder;
            }
        }
        await _dbContext.SaveChangesAsync();
    }
}
