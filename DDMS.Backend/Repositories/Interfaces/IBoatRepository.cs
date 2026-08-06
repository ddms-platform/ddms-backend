using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBoatRepository
{
    Task<(List<boat> items, int total)> GetPagedAsync(BoatListQuery query);
    Task<List<boat>> GetAllAsync();
    Task<boat?> GetByIdAsync(Guid id);
    Task<boat?> GetByIdWithDetailsAsync(Guid id);
    Task<BoatStatsResponse> GetStatsAsync();
    Task<boat> CreateAsync(boat entity);
    Task UpdateAsync(boat entity);
    Task DeleteAsync(boat entity);

    // Owner-specific
    Task<(List<boat> items, int total)> GetPagedByOwnerAsync(Guid ownerId, OwnerBoatListQuery query);
    Task<boat?> GetByIdAndOwnerAsync(Guid id, Guid ownerId);
    Task<BoatStatsResponse> GetStatsByOwnerAsync(Guid ownerId);
    Task<tour?> GetTourForBoatOwnerAsync(Guid boatId, Guid tourId, Guid ownerId);
    Task<boat_service?> GetBoatServiceForOwnerAsync(Guid boatId, Guid serviceId, Guid ownerId);
    Task DetachTourFromBoatAsync(tour entity, Guid boatId);
    Task DeleteBoatServiceAsync(boat_service entity);
}
