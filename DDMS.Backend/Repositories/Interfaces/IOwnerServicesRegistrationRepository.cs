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
    Task SaveChangesAsync(CancellationToken ct);
}
