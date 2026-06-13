using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IPublicTourSearchRepository
{
    Task<(List<tour> items, int total)> SearchAsync(TourSearchQuery query);
    Task<Dictionary<Guid, int>> GetBookedCapacityByScheduleIdsAsync(IEnumerable<Guid> scheduleIds);
    Task<List<PopularDestinationResponse>> GetPopularDestinationsAsync(int limit);
}
