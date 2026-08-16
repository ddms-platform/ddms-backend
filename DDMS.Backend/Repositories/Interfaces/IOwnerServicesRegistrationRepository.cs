using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IOwnerServicesRegistrationRepository
{
    void AddBoatCabin(boat_cabin entity);
    void AddBoatService(boat_service entity);
    void AddFaq(faq entity);
    void AddRoute(route entity);
    void AddTourSchedule(tour_schedule entity);
    Task<boat?> FindBoatWithOwnerAsync(Guid boatId, CancellationToken ct);
    Task<tour?> FindTourByIdAsync(Guid tourId, CancellationToken ct);
    Task<bool> HasScheduleForBoatAndTourAsync(Guid boatId, Guid tourId, CancellationToken ct);
    Task RemoveFaqsByTourIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveRoutesByTourIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveCabinsByBoatIdAsync(Guid boatId, CancellationToken ct);
    Task RemoveCombosByBoatIdAsync(Guid boatId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
