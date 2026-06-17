using DDMS.Backend.Models.DTOs.MaintenanceServices;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class MaintenanceServicesService : IMaintenanceServicesService
{
    private readonly IMaintenanceServicesRepository _repo;
    public MaintenanceServicesService(IMaintenanceServicesRepository repo) => _repo = repo;

    public Task<List<MaintenanceServiceItem>> GetAllAsync(CancellationToken ct) =>
        _repo.GetAllOrderedByPriceAsync(ct);
}
