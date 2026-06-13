using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Services.Interfaces;

public interface IPublicTourSearchService
{
    Task<PagedResponse<TourSearchItemResponse>> SearchAsync(TourSearchQuery query);
    Task<List<PopularDestinationResponse>> GetPopularDestinationsAsync(int limit);
}
