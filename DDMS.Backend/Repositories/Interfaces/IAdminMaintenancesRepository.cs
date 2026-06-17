using DDMS.Backend.Models.DTOs.AdminMaintenances;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IAdminMaintenancesRepository
{
    Task<List<MaintenanceItem>> GetByStatusAsync(string? status, CancellationToken ct);
    Task<boat_maintenance?> FindWithBoatOwnerAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
