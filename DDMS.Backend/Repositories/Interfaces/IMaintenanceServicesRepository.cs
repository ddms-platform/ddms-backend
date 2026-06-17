using DDMS.Backend.Models.DTOs.MaintenanceServices;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IMaintenanceServicesRepository
{
    Task<List<MaintenanceServiceItem>> GetAllOrderedByPriceAsync(CancellationToken ct);
}
