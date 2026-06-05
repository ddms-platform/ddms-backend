using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class TourRepository : ITourRepository
{
    private readonly AppDbContext _db;

    public TourRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<tour?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.tours.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
    }

    public async Task<List<tour>> GetListAsync(string? status, string? location, CancellationToken cancellationToken)
    {
        var query = _db.tours.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.status == status);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(x => x.location != null && x.location.Contains(location));
        }

        return await query
            .AsNoTracking()
            .OrderByDescending(x => x.created_at)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(tour tour, CancellationToken cancellationToken)
    {
        await _db.tours.AddAsync(tour, cancellationToken);
    }

    public void Update(tour tour)
    {
        _db.tours.Update(tour);
    }

    public void Delete(tour tour)
    {
        _db.tours.Remove(tour);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.tours.AnyAsync(x => x.id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
