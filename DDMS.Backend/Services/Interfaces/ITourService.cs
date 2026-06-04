using DDMS.Backend.Models.DTOs.Tour;

namespace DDMS.Backend.Services.Interfaces;

public interface ITourService
{
    Task<TourResponse> CreateAsync(CreateTourRequest request, CancellationToken cancellationToken);
    Task<TourResponse> UpdateAsync(Guid id, UpdateTourRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<TourResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TourResponse>> GetListAsync(TourFilterRequest request, CancellationToken cancellationToken);
}
