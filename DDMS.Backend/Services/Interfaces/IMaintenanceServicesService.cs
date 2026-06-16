using DDMS.Backend.Models.DTOs.MaintenanceServices;

namespace DDMS.Backend.Services.Interfaces;

public interface IMaintenanceServicesService
{
    Task<List<MaintenanceServiceItem>> GetAllAsync(CancellationToken ct);
}
