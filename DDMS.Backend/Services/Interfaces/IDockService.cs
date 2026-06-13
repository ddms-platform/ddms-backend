using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Dock;

namespace DDMS.Backend.Services.Interfaces;

public interface IDockService
{
    Task<PagedResponse<DockListItemResponse>> GetDocksAsync(DockListQuery query);
    Task<List<DockListItemResponse>> GetAllAsync();
    Task<DockListItemResponse> GetByIdAsync(Guid id);
    Task<DockStatsResponse> GetStatsAsync();
    Task<DockListItemResponse> CreateAsync(CreateDockRequest request);
    Task<DockListItemResponse> UpdateAsync(Guid id, UpdateDockRequest request);
    Task DeleteAsync(Guid id);

    Task<List<DockScheduleResponse>> GetSchedulesAsync(Guid dockId);
    Task<DockScheduleResponse> AddScheduleAsync(Guid dockId, CreateDockScheduleRequest request);
    Task DeleteScheduleAsync(Guid dockId, Guid scheduleId);
}
