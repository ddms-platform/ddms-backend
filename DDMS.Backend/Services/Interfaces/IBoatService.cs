using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;

namespace DDMS.Backend.Services.Interfaces;

public interface IBoatService
{
    Task<PagedResponse<BoatListItemResponse>> GetBoatsAsync(BoatListQuery query);
    Task<List<BoatListItemResponse>> GetAllAsync();
    Task<BoatDetailResponse> GetByIdAsync(Guid id);
    Task<BoatStatsResponse> GetStatsAsync();
    Task<BoatDetailResponse> CreateAsync(CreateBoatRequest request);
    Task<BoatDetailResponse> UpdateAsync(Guid id, UpdateBoatRequest request);
    Task DeleteAsync(Guid id);

    // Owner-specific
    Task<PagedResponse<BoatListItemResponse>> GetBoatsByOwnerAsync(Guid ownerId, OwnerBoatListQuery query);
    Task<BoatDetailResponse> GetByIdByOwnerAsync(Guid id, Guid ownerId);
    Task<BoatStatsResponse> GetStatsByOwnerAsync(Guid ownerId);
    Task<BoatDetailResponse> CreateByOwnerAsync(CreateBoatRequest request, Guid ownerId);
    Task<BoatDetailResponse> UpdateByOwnerAsync(Guid id, UpdateBoatRequest request, Guid ownerId);
    Task DeleteByOwnerAsync(Guid id, Guid ownerId);
}
