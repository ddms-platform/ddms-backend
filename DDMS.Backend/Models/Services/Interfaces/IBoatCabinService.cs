using DDMS.Backend.Models.DTOs.Boat;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IBoatCabinService
{
    Task<List<BoatCabinResponse>> GetByBoatIdAsync(Guid boatId);
    Task<BoatCabinResponse> GetByIdAsync(Guid boatId, Guid id);
    Task<BoatCabinResponse> CreateAsync(Guid boatId, CreateBoatCabinRequest request);
    Task<BoatCabinResponse> UpdateAsync(Guid boatId, Guid id, UpdateBoatCabinRequest request);
    Task DeleteAsync(Guid boatId, Guid id);
}
