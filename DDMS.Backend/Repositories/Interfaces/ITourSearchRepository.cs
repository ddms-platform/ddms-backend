using DDMS.Backend.Models.DTOs.TourSearch;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ITourSearchRepository
{
    Task<List<TourSearchResponse>> SearchAsync(TourSearchRequest request, CancellationToken cancellationToken);
}
