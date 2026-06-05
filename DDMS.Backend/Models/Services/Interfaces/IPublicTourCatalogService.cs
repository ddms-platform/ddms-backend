using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IPublicTourCatalogService
{
    Task<TourItemResponse> GetActiveTourAsync(Guid tourId);
    Task<List<TourImageItemResponse>> GetTourImagesAsync(Guid tourId);
    Task<List<FaqItemResponse>> GetTourFaqsAsync(Guid tourId);
}
