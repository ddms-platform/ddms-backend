using DDMS.Backend.Models.DTOs.Boat;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IBoatImageService
{
    Task<List<BoatImageResponse>> GetByBoatIdAsync(Guid boatId);
    Task<BoatImageResponse> UploadAsync(Guid boatId, string fileBase64, string? caption);
    Task DeleteAsync(Guid boatId, Guid imageId);
}
