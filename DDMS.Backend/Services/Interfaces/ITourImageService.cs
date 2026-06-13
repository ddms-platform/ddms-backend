using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Services.Interfaces;

public interface ITourImageService
{
    Task<List<TourImageItemResponse>> GetByTourIdAsync(Guid tourId, Guid userId);
    Task<TourImageItemResponse> UploadAsync(Guid tourId, Guid userId, UploadTourImageRequest request);
    Task<TourImageItemResponse> UpdateAsync(Guid tourId, Guid imageId, Guid userId, UpdateTourImageRequest request);
    Task DeleteAsync(Guid tourId, Guid imageId, Guid userId);
    Task<TourItemResponse> UpdateContentAsync(Guid tourId, Guid userId, UpdateTourContentRequest request);
}
