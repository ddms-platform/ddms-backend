using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class BoatAddonRepository : IBoatAddonRepository
{
    private readonly AppDbContext _dbContext;

    public BoatAddonRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<boat_service>> GetByBoatIdAsync(Guid boatId)
    {
        return await _dbContext.boat_services
            .Where(s => s.boat_id == boatId)
            .OrderBy(s => s.created_at)
            .ToListAsync();
    }

    public Task<boat_service?> GetByIdAsync(Guid id)
    {
        return _dbContext.boat_services
            .FirstOrDefaultAsync(s => s.id == id);
    }

    public async Task<boat_service> CreateAsync(boat_service entity)
    {
        _dbContext.boat_services.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(boat_service entity)
    {
        _dbContext.boat_services.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(boat_service entity)
    {
        _dbContext.boat_services.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
