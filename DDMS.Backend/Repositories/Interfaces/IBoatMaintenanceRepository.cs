using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBoatMaintenanceRepository
{
    Task<port_maintenance_service?> FindServiceAsync(Guid serviceId, CancellationToken ct);
    void AddMaintenance(boat_maintenance entity);
    Task<boat_maintenance?> FindMaintenanceForBoatAsync(Guid maintenanceId, Guid boatId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
