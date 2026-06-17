using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IDockRepository
{
    Task<(List<dock> items, int total)> GetPagedAsync(DockListQuery query);
    Task<List<dock>> GetAllAsync();
    Task<dock?> GetByIdAsync(Guid id);
    Task<DockStatsResponse> GetStatsAsync();
    Task<dock> CreateAsync(dock entity);
    Task UpdateAsync(dock entity);
    Task DeleteAsync(dock entity);

    Task<List<dock_schedule>> GetSchedulesAsync(Guid dockId);
    Task<dock_schedule?> GetScheduleAsync(Guid dockId, Guid scheduleId);
    Task<dock_schedule> AddScheduleAsync(dock_schedule entity);
    Task DeleteScheduleAsync(dock_schedule entity);
    Task<bool> HasConflictAsync(Guid dockId, Guid boatId, DateTime startTime, DateTime endTime, Guid? excludeId = null);
    Task<int> CountActiveSchedulesAsync(Guid dockId, DateTime at);
}
