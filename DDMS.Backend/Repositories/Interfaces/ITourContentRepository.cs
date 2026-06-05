using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ITourContentRepository
{
    Task<bool> ExistsTourAsync(Guid tourId, CancellationToken cancellationToken);

    Task<tour_image?> GetImageByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<tour_image>> GetImagesByTourIdAsync(Guid tourId, CancellationToken cancellationToken);
    Task AddImageAsync(tour_image image, CancellationToken cancellationToken);
    void UpdateImage(tour_image image);
    void DeleteImage(tour_image image);

    Task<faq?> GetFaqByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<faq>> GetFaqsByTourIdAsync(Guid tourId, CancellationToken cancellationToken);
    Task AddFaqAsync(faq faq, CancellationToken cancellationToken);
    void UpdateFaq(faq faq);
    void DeleteFaq(faq faq);

    Task<dock_schedule?> GetDockScheduleByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<dock_schedule>> GetDockSchedulesByDockIdAsync(Guid dockId, CancellationToken cancellationToken);
    Task AddDockScheduleAsync(dock_schedule dockSchedule, CancellationToken cancellationToken);
    void UpdateDockSchedule(dock_schedule dockSchedule);
    void DeleteDockSchedule(dock_schedule dockSchedule);
    Task<bool> ExistsDockAsync(Guid dockId, CancellationToken cancellationToken);
    Task<bool> ExistsBoatAsync(Guid boatId, CancellationToken cancellationToken);
    Task<bool> HasOverlapAsync(Guid dockId, DateTime startTime, DateTime endTime, Guid? excludeId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
