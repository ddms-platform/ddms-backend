using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBoatImageRepository
{
    Task<List<boat_image>> GetByBoatIdAsync(Guid boatId);
    Task<boat_image?> GetByIdAsync(Guid id);
    Task<boat_image> CreateAsync(boat_image entity);
    Task DeleteAsync(boat_image entity);
    Task UpdateSortOrderAsync(List<(Guid id, int sortOrder)> updates);
}
