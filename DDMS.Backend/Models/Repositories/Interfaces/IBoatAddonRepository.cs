using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IBoatAddonRepository
{
    Task<List<boat_service>> GetByBoatIdAsync(Guid boatId);
    Task<boat_service?> GetByIdAsync(Guid id);
    Task<boat_service> CreateAsync(boat_service entity);
    Task UpdateAsync(boat_service entity);
    Task DeleteAsync(boat_service entity);
}
