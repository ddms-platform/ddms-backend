using DDMS.Backend.Models.DTOs.OwnerBoats;

namespace DDMS.Backend.Services.Interfaces;

public interface IBoatMaintenanceService
{
    Task<int> RegisterAsync(Guid boatId, Guid ownerId, IReadOnlyList<MaintenanceRegistrationRequest> registrations, CancellationToken ct);
    Task DeleteAsync(Guid boatId, Guid maintenanceId, Guid ownerId, CancellationToken ct);
}
