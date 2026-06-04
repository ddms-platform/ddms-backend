using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class TourScheduleRepository : ITourScheduleRepository
{
    private readonly AppDbContext _db;

    public TourScheduleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<tour_schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.tour_schedules.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
    }

    public async Task<List<tour_schedule>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.tour_schedules
            .AsNoTracking()
            .Where(x => x.tour_id == tourId)
            .OrderBy(x => x.start_time)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(tour_schedule schedule, CancellationToken cancellationToken)
    {
        await _db.tour_schedules.AddAsync(schedule, cancellationToken);
    }

    public void Update(tour_schedule schedule)
    {
        _db.tour_schedules.Update(schedule);
    }

    public void Delete(tour_schedule schedule)
    {
        _db.tour_schedules.Remove(schedule);
    }

    public async Task<bool> ExistsTourAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.tours.AnyAsync(x => x.id == tourId, cancellationToken);
    }

    public async Task<bool> ExistsBoatAsync(Guid boatId, CancellationToken cancellationToken)
    {
        return await _db.boats.AnyAsync(x => x.id == boatId, cancellationToken);
    }

    public async Task<bool> ExistsDockAsync(Guid dockId, CancellationToken cancellationToken)
    {
        return await _db.docks.AnyAsync(x => x.id == dockId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
