using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBoatCabinRepository
{
    Task<List<boat_cabin>> GetByBoatIdAsync(Guid boatId);
    Task<boat_cabin?> GetByIdAsync(Guid id);
    Task<boat_cabin> CreateAsync(boat_cabin entity);
    Task UpdateAsync(boat_cabin entity);
    Task DeleteAsync(boat_cabin entity);
}
