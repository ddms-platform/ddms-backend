using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class RouteRepository : IRouteRepository
{
    private readonly AppDbContext _db;

    public RouteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<route?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.routes.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
    }

    public async Task<List<route>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.routes
            .AsNoTracking()
            .Where(x => x.tour_id == tourId)
            .OrderBy(x => x.sort_order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(route route, CancellationToken cancellationToken)
    {
        await _db.routes.AddAsync(route, cancellationToken);
    }

    public void Update(route route)
    {
        _db.routes.Update(route);
    }

    public void Delete(route route)
    {
        _db.routes.Remove(route);
    }

    public async Task<bool> ExistsTourAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.tours.AnyAsync(x => x.id == tourId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
