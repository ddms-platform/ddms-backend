using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class BoatCabinRepository : IBoatCabinRepository
{
    private readonly AppDbContext _dbContext;

    public BoatCabinRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<boat_cabin>> GetByBoatIdAsync(Guid boatId)
    {
        return await _dbContext.boat_cabins
            .Where(c => c.boat_id == boatId)
            .OrderBy(c => c.created_at)
            .ToListAsync();
    }

    public Task<boat_cabin?> GetByIdAsync(Guid id)
    {
        return _dbContext.boat_cabins
            .FirstOrDefaultAsync(c => c.id == id);
    }

    public async Task<boat_cabin> CreateAsync(boat_cabin entity)
    {
        _dbContext.boat_cabins.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(boat_cabin entity)
    {
        _dbContext.boat_cabins.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(boat_cabin entity)
    {
        _dbContext.boat_cabins.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
