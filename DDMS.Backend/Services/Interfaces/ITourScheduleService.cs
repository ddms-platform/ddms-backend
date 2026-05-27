using DDMS.Backend.Models.DTOs.TourSchedule;

namespace DDMS.Backend.Services.Interfaces;

public interface ITourScheduleService
{
    Task<TourScheduleResponse> CreateAsync(CreateTourScheduleRequest request, CancellationToken cancellationToken);
    Task<TourScheduleResponse> UpdateAsync(Guid id, UpdateTourScheduleRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TourScheduleResponse>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken);
}
