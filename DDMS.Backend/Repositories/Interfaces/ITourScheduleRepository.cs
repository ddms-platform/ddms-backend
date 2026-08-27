using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ITourScheduleRepository
{
    Task<tour_schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<tour_schedule>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken);
    Task AddAsync(tour_schedule schedule, CancellationToken cancellationToken);
    void Update(tour_schedule schedule);
    void Delete(tour_schedule schedule);
    Task<bool> ExistsTourAsync(Guid tourId, CancellationToken cancellationToken);
    Task<bool> ExistsBoatAsync(Guid boatId, CancellationToken cancellationToken);
    Task<bool> ExistsDockAsync(Guid dockId, CancellationToken cancellationToken);
    Task<Guid?> GetBoatOwnerIdAsync(Guid boatId, CancellationToken cancellationToken);
    Task<Guid?> GetTourCreatedByAsync(Guid tourId, CancellationToken cancellationToken);
    Task<bool> HasBoatScheduleOverlapAsync(
        Guid boatId, DateTime start, DateTime end, Guid? excludeScheduleId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
