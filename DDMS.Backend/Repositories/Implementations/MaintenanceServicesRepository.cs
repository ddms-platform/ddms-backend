using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.MaintenanceServices;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class MaintenanceServicesRepository : IMaintenanceServicesRepository
{
    private readonly AppDbContext _db;
    public MaintenanceServicesRepository(AppDbContext db) => _db = db;

    public Task<List<MaintenanceServiceItem>> GetAllOrderedByPriceAsync(CancellationToken ct) =>
        _db.port_maintenance_services
            .OrderBy(s => s.price ?? decimal.MaxValue)
            .Select(s => new MaintenanceServiceItem
            {
                Id = s.id,
                Name = s.name,
                IconCode = s.icon_code,
                Price = s.price,
                Description = s.description
            })
            .ToListAsync(ct);
}
