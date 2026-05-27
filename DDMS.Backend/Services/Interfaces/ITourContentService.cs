using DDMS.Backend.Models.DTOs.TourContent;

namespace DDMS.Backend.Services.Interfaces;

public interface ITourContentService
{
    Task<TourImageResponse> CreateImageAsync(CreateTourImageRequest request, CancellationToken cancellationToken);
    Task<TourImageResponse> UpdateImageAsync(Guid id, UpdateTourImageRequest request, CancellationToken cancellationToken);
    Task DeleteImageAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TourImageResponse>> GetImagesByTourIdAsync(Guid tourId, CancellationToken cancellationToken);

    Task<FaqResponse> CreateFaqAsync(CreateFaqRequest request, CancellationToken cancellationToken);
    Task<FaqResponse> UpdateFaqAsync(Guid id, UpdateFaqRequest request, CancellationToken cancellationToken);
    Task DeleteFaqAsync(Guid id, CancellationToken cancellationToken);
    Task<List<FaqResponse>> GetFaqsByTourIdAsync(Guid tourId, CancellationToken cancellationToken);

    Task<DockScheduleResponse> CreateDockScheduleAsync(CreateDockScheduleRequest request, CancellationToken cancellationToken);
    Task<DockScheduleResponse> UpdateDockScheduleAsync(Guid id, UpdateDockScheduleRequest request, CancellationToken cancellationToken);
    Task DeleteDockScheduleAsync(Guid id, CancellationToken cancellationToken);
    Task<List<DockScheduleResponse>> GetDockSchedulesByDockIdAsync(Guid dockId, CancellationToken cancellationToken);
}
