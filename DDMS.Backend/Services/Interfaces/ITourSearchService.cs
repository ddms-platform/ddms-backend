using DDMS.Backend.Models.DTOs.TourSearch;

namespace DDMS.Backend.Services.Interfaces;

public interface ITourSearchService
{
    Task<List<TourSearchResponse>> SearchAsync(TourSearchRequest request, CancellationToken cancellationToken);
}
