using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IOwnerServicesRegistrationRepository
{
    void AddBoatCabin(boat_cabin entity);
    void AddBoatService(boat_service entity);
    void AddFaq(faq entity);
    void AddRoute(route entity);
    void AddTourImage(tour_image entity);
    Task<boat?> FindBoatWithOwnerAsync(Guid boatId, CancellationToken ct);
    Task<tour?> FindTourByIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveFaqsByTourIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveRoutesByTourIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveImagesByTourIdAsync(Guid tourId, CancellationToken ct);
    /// <summary>
    /// Chi xoa phong/combo cua dung tour do. Ban theo boatId cu van giu de
    /// khong pha code khac, nhung luong dang ky dich vu khong dung nua: no xoa
    /// ca phong cua nhung tour khac chay tren cung con thuyen.
    /// </summary>
    Task RemoveCabinsByTourIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveCombosByTourIdAsync(Guid tourId, CancellationToken ct);
    Task RemoveCabinsByBoatIdAsync(Guid boatId, CancellationToken ct);
    Task RemoveCombosByBoatIdAsync(Guid boatId, CancellationToken ct);
    void AddChangeRequest(service_change_request entity);
    Task<service_change_request?> FindPendingChangeByTourIdAsync(Guid tourId, CancellationToken ct);
    Task<service_change_request?> FindChangeByIdAsync(Guid id, CancellationToken ct);
    Task<List<service_change_request>> ListChangesAsync(string? status, CancellationToken ct);
    Task<HashSet<Guid>> GetPendingChangeTourIdsAsync(IEnumerable<Guid> tourIds, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
