using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BoatMaintenanceRepository : IBoatMaintenanceRepository
{
    private readonly AppDbContext _db;
    public BoatMaintenanceRepository(AppDbContext db) => _db = db;

    public Task<port_maintenance_service?> FindServiceAsync(Guid serviceId, CancellationToken ct) =>
        _db.port_maintenance_services.FindAsync(new object?[] { serviceId }, ct).AsTask();

    public void AddMaintenance(boat_maintenance entity) => _db.boat_maintenances.Add(entity);

    public Task<boat_maintenance?> FindMaintenanceForBoatAsync(Guid maintenanceId, Guid boatId, CancellationToken ct) =>
        _db.boat_maintenances.FirstOrDefaultAsync(m => m.id == maintenanceId && m.boat_id == boatId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
