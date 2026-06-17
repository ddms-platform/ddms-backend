using DDMS.Backend.Models.DTOs.AdminMaintenances;

namespace DDMS.Backend.Services.Interfaces;

public interface IAdminMaintenancesService
{
    Task<List<MaintenanceItem>> GetPendingAsync(CancellationToken ct);
    Task<List<MaintenanceItem>> GetAllAsync(CancellationToken ct);
    Task ApproveAsync(Guid id, CancellationToken ct);
    Task RejectAsync(Guid id, CancellationToken ct);
}
