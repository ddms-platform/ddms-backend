using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IDockScheduleRepository
{
    Task<(List<dock_schedule> items, int total)> GetPagedAsync(DockScheduleListQuery query);
    Task<dock_schedule?> GetByIdAsync(Guid id);
    Task<dock?> GetDockAsync(Guid dockId);
    Task<boat?> GetBoatAsync(Guid boatId);
    Task<bool> HasBoatOverlapAsync(Guid boatId, DateTime startTime, DateTime endTime, Guid? excludeId = null);
    Task<int> CountDockOverlapsAsync(Guid dockId, DateTime startTime, DateTime endTime, Guid? excludeId = null);
    Task AddAsync(dock_schedule entity);
    Task UpdateAsync(dock_schedule entity);
    Task DeleteAsync(dock_schedule entity);
}
