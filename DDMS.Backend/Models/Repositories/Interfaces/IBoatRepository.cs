using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

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
}
