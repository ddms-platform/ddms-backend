using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IBoatService
{
    Task<PagedResponse<BoatListItemResponse>> GetBoatsAsync(BoatListQuery query);
    Task<List<BoatListItemResponse>> GetAllAsync();
    Task<BoatDetailResponse> GetByIdAsync(Guid id);
    Task<BoatStatsResponse> GetStatsAsync();
    Task<BoatDetailResponse> CreateAsync(CreateBoatRequest request);
    Task<BoatDetailResponse> UpdateAsync(Guid id, UpdateBoatRequest request);
    Task DeleteAsync(Guid id);
}
