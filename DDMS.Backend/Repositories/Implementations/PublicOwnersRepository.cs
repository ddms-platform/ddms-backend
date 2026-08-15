using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class PublicOwnersRepository : IPublicOwnersRepository
{
    private readonly AppDbContext _db;

    public PublicOwnersRepository(AppDbContext db) => _db = db;

    public Task<List<owner_profile>> GetVerifiedProfilesAsync(int take, CancellationToken ct) =>
        _db.owner_profiles
            .Include(op => op.user)
            .Where(op => op.is_verified && op.status == OwnerProfileStatuses.Verified)
            .OrderByDescending(op => op.verified_at ?? op.created_at)
            .Take(take)
            .ToListAsync(ct);

    public Task<List<boat>> GetActiveBoatsWithImagesAsync(
        IReadOnlyCollection<Guid> ownerIds, CancellationToken ct)
    {
        if (ownerIds.Count == 0) return Task.FromResult(new List<boat>());

        return _db.boats
            .Include(b => b.boat_images)
            .Where(b => b.owner_id != null
                     && ownerIds.Contains(b.owner_id.Value)
                     && !b.is_deleted
                     && b.status != BoatStatuses.Rejected
                     && b.status != BoatStatuses.Pending)
            .ToListAsync(ct);
    }

    public async Task<List<(Guid BoatId, Guid TourId)>> GetTourIdsByBoatAsync(
        IReadOnlyCollection<Guid> boatIds, CancellationToken ct)
    {
        if (boatIds.Count == 0) return new List<(Guid, Guid)>();

        var rows = await _db.tour_schedules
            .Where(s => s.boat_id != null && boatIds.Contains(s.boat_id.Value))
            .Select(s => new { BoatId = s.boat_id!.Value, s.tour_id })
            .Distinct()
            .ToListAsync(ct);

        return rows.Select(r => (r.BoatId, r.tour_id)).ToList();
    }

    public async Task<List<(Guid TourId, double AvgRating, int ReviewCount)>> GetRatingsByTourAsync(
        IReadOnlyCollection<Guid> tourIds, CancellationToken ct)
    {
        if (tourIds.Count == 0) return new List<(Guid, double, int)>();

        var rows = await _db.reviews
            .Where(r => tourIds.Contains(r.tour_id))
            .GroupBy(r => r.tour_id)
            .Select(g => new
            {
                TourId = g.Key,
                Avg = g.Average(x => (double)x.rating),
                Count = g.Count(),
            })
            .ToListAsync(ct);

        return rows.Select(r => (r.TourId, r.Avg, r.Count)).ToList();
    }
}
