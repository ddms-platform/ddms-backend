using DDMS.Backend.Models.DTOs.TourSearch;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourSearchService : ITourSearchService
{
    private readonly ITourSearchRepository _repository;

    public TourSearchService(ITourSearchRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TourSearchResponse>> SearchAsync(TourSearchRequest request, CancellationToken cancellationToken)
    {
        return await _repository.SearchAsync(request, cancellationToken);
    }
}
