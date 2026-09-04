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
    public void AddTourImage(tour_image entity) => _db.tour_images.Add(entity);

    public Task<boat?> FindBoatWithOwnerAsync(Guid boatId, CancellationToken ct) =>
        _db.boats.Include(b => b.owner).FirstOrDefaultAsync(b => b.id == boatId, ct);

    public Task<tour?> FindTourByIdAsync(Guid tourId, CancellationToken ct) =>
        _db.tours.FirstOrDefaultAsync(t => t.id == tourId, ct);

    public async Task RemoveFaqsByTourIdAsync(Guid tourId, CancellationToken ct)
    {
        var faqs = await _db.faqs.Where(f => f.tour_id == tourId).ToListAsync(ct);
        if (faqs.Count > 0)
        {
            _db.faqs.RemoveRange(faqs);
        }
    }

    public async Task RemoveRoutesByTourIdAsync(Guid tourId, CancellationToken ct)
    {
        var routes = await _db.routes.Where(r => r.tour_id == tourId).ToListAsync(ct);
        if (routes.Count > 0)
        {
            _db.routes.RemoveRange(routes);
        }
    }

    public async Task RemoveImagesByTourIdAsync(Guid tourId, CancellationToken ct)
    {
        var images = await _db.tour_images.Where(i => i.tour_id == tourId).ToListAsync(ct);
        if (images.Count > 0)
        {
            _db.tour_images.RemoveRange(images);
        }
    }

    public async Task RemoveCabinsByTourIdAsync(Guid tourId, CancellationToken ct)
    {
        var cabins = await _db.boat_cabins.Where(c => c.tour_id == tourId).ToListAsync(ct);
        if (cabins.Count > 0)
        {
            _db.boat_cabins.RemoveRange(cabins);
        }
    }

    public async Task RemoveCombosByTourIdAsync(Guid tourId, CancellationToken ct)
    {
        var combos = await _db.boat_services.Where(s => s.tour_id == tourId).ToListAsync(ct);
        if (combos.Count > 0)
        {
            _db.boat_services.RemoveRange(combos);
        }
    }

    public async Task RemoveCabinsByBoatIdAsync(Guid boatId, CancellationToken ct)
    {
        var cabins = await _db.boat_cabins.Where(c => c.boat_id == boatId).ToListAsync(ct);
        if (cabins.Count > 0)
        {
            _db.boat_cabins.RemoveRange(cabins);
        }
    }

    public async Task RemoveCombosByBoatIdAsync(Guid boatId, CancellationToken ct)
    {
        var combos = await _db.boat_services.Where(s => s.boat_id == boatId).ToListAsync(ct);
        if (combos.Count > 0)
        {
            _db.boat_services.RemoveRange(combos);
        }
    }

    public void AddChangeRequest(service_change_request entity) =>
        _db.service_change_requests.Add(entity);

    public Task<service_change_request?> FindPendingChangeByTourIdAsync(
        Guid tourId, CancellationToken ct) =>
        _db.service_change_requests.FirstOrDefaultAsync(
            x => x.tour_id == tourId && x.status == "pending", ct);

    public Task<service_change_request?> FindChangeByIdAsync(Guid id, CancellationToken ct) =>
        _db.service_change_requests
            .Include(x => x.tour)
            .Include(x => x.boat)
            .FirstOrDefaultAsync(x => x.id == id, ct);

    public async Task<List<service_change_request>> ListChangesAsync(
        string? status, CancellationToken ct)
    {
        var query = _db.service_change_requests
            .Include(x => x.tour)
            .Include(x => x.boat)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.status == normalized);
        }

        return await query
            .OrderByDescending(x => x.created_at)
            .ToListAsync(ct);
    }

    public async Task<HashSet<Guid>> GetPendingChangeTourIdsAsync(
        IEnumerable<Guid> tourIds, CancellationToken ct)
    {
        var ids = tourIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        var pending = await _db.service_change_requests
            .Where(x => ids.Contains(x.tour_id) && x.status == "pending")
            .Select(x => x.tour_id)
            .ToListAsync(ct);
        return pending.ToHashSet();
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
