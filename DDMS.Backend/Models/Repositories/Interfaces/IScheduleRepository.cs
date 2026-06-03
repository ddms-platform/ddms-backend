using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IScheduleRepository
{
    Task<(List<tour_schedule> items, int total)> GetPagedAsync(Guid userId, ScheduleListQuery query);
    Task<tour_schedule?> GetByIdAsync(Guid id, Guid userId);
    Task<bool> BoatExistsAsync(Guid boatId);
    Task<bool> DockExistsAsync(Guid dockId);
    Task AddAsync(tour_schedule entity);
    Task UpdateAsync(tour_schedule entity);
}
