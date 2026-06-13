using DDMS.Backend.Models.DTOs.Boat;

namespace DDMS.Backend.Services.Interfaces;

public interface IBoatAddonService
{
    Task<List<BoatServiceResponse>> GetByBoatIdAsync(Guid boatId);
    Task<BoatServiceResponse> GetByIdAsync(Guid boatId, Guid id);
    Task<BoatServiceResponse> CreateAsync(Guid boatId, CreateBoatServiceRequest request);
    Task<BoatServiceResponse> UpdateAsync(Guid boatId, Guid id, UpdateBoatServiceRequest request);
    Task<BoatServiceResponse> ToggleActiveAsync(Guid boatId, Guid id);
    Task DeleteAsync(Guid boatId, Guid id);
}
