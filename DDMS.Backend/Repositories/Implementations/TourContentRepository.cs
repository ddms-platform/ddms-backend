using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class TourContentRepository : ITourContentRepository
{
    private readonly AppDbContext _db;

    public TourContentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsTourAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.tours.AnyAsync(x => x.id == tourId, cancellationToken);
    }

    public async Task<tour_image?> GetImageByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.tour_images.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
    }

    public async Task<List<tour_image>> GetImagesByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.tour_images
            .AsNoTracking()
            .Where(x => x.tour_id == tourId)
            .OrderBy(x => x.sort_order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddImageAsync(tour_image image, CancellationToken cancellationToken)
    {
        await _db.tour_images.AddAsync(image, cancellationToken);
    }

    public void UpdateImage(tour_image image)
    {
        _db.tour_images.Update(image);
    }

    public void DeleteImage(tour_image image)
    {
        _db.tour_images.Remove(image);
    }

    public async Task<faq?> GetFaqByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.faqs.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
    }

    public async Task<List<faq>> GetFaqsByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        return await _db.faqs
            .AsNoTracking()
            .Where(x => x.tour_id == tourId)
            .OrderBy(x => x.sort_order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddFaqAsync(faq faq, CancellationToken cancellationToken)
    {
        await _db.faqs.AddAsync(faq, cancellationToken);
    }

    public void UpdateFaq(faq faq)
    {
        _db.faqs.Update(faq);
    }

    public void DeleteFaq(faq faq)
    {
        _db.faqs.Remove(faq);
    }

    public async Task<dock_schedule?> GetDockScheduleByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.dock_schedules.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
    }

    public async Task<List<dock_schedule>> GetDockSchedulesByDockIdAsync(Guid dockId, CancellationToken cancellationToken)
    {
        return await _db.dock_schedules
            .AsNoTracking()
            .Where(x => x.dock_id == dockId)
            .OrderBy(x => x.start_time)
            .ToListAsync(cancellationToken);
    }

    public async Task AddDockScheduleAsync(dock_schedule dockSchedule, CancellationToken cancellationToken)
    {
        await _db.dock_schedules.AddAsync(dockSchedule, cancellationToken);
    }

    public void UpdateDockSchedule(dock_schedule dockSchedule)
    {
        _db.dock_schedules.Update(dockSchedule);
    }

    public void DeleteDockSchedule(dock_schedule dockSchedule)
    {
        _db.dock_schedules.Remove(dockSchedule);
    }

    public async Task<bool> ExistsDockAsync(Guid dockId, CancellationToken cancellationToken)
    {
        return await _db.docks.AnyAsync(x => x.id == dockId, cancellationToken);
    }

    public async Task<bool> ExistsBoatAsync(Guid boatId, CancellationToken cancellationToken)
    {
        return await _db.boats.AnyAsync(x => x.id == boatId, cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(Guid dockId, DateTime startTime, DateTime endTime, Guid? excludeId, CancellationToken cancellationToken)
    {
        return await _db.dock_schedules.AnyAsync(
            x => x.dock_id == dockId
                 && (excludeId == null || x.id != excludeId.Value)
                 && x.start_time < endTime
                 && startTime < x.end_time,
            cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
