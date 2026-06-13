using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class OwnerServicesRegistrationRepository : IOwnerServicesRegistrationRepository
{
    private readonly AppDbContext _db;
    public OwnerServicesRegistrationRepository(AppDbContext db) => _db = db;

    public void AddBoatCabin(boat_cabin entity) => _db.boat_cabins.Add(entity);
    public void AddBoatService(boat_service entity) => _db.boat_services.Add(entity);
    public void AddFaq(faq entity) => _db.faqs.Add(entity);
    public void AddRoute(route entity) => _db.routes.Add(entity);
    public void AddTourSchedule(tour_schedule entity) => _db.tour_schedules.Add(entity);

    public Task<boat?> FindBoatWithOwnerAsync(Guid boatId, CancellationToken ct) =>
        _db.boats.Include(b => b.owner).FirstOrDefaultAsync(b => b.id == boatId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
