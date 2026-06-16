using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.AdminMaintenances;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class AdminMaintenancesRepository : IAdminMaintenancesRepository
{
    private readonly AppDbContext _db;
    public AdminMaintenancesRepository(AppDbContext db) => _db = db;

    public Task<List<MaintenanceItem>> GetByStatusAsync(string? status, CancellationToken ct)
    {
        var q = _db.boat_maintenances
            .Include(m => m.boat)
            .Include(m => m.port_maintenance_service)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status)) q = q.Where(m => m.status == status);

        return q.OrderByDescending(m => m.created_at)
            .Select(m => new MaintenanceItem
            {
                Id = m.id,
                BoatId = m.boat_id,
                BoatName = m.boat.name,
                StartTime = m.start_time,
                EndTime = m.end_time,
                Reason = m.reason,
                CreatedAt = m.created_at,
                PortMaintenanceServiceId = m.port_maintenance_service_id,
                PortMaintenanceServiceName = m.port_maintenance_service != null ? m.port_maintenance_service.name : m.reason,
                Price = m.port_maintenance_service != null ? m.port_maintenance_service.price : null,
                Status = m.status
            })
            .ToListAsync(ct);
    }

    public Task<boat_maintenance?> FindWithBoatOwnerAsync(Guid id, CancellationToken ct) =>
        _db.boat_maintenances
            .Include(x => x.boat).ThenInclude(b => b.owner)
            .Include(x => x.port_maintenance_service)
            .FirstOrDefaultAsync(x => x.id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
