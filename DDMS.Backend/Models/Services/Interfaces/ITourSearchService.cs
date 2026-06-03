using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface ITourSearchService
{
    Task<PagedResponse<TourSearchItemResponse>> SearchAsync(TourSearchQuery query);
}
