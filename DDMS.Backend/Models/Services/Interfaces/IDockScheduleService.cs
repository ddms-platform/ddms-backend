using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IDockScheduleService
{
    Task<PagedResponse<DockScheduleItemResponse>> GetSchedulesAsync(DockScheduleListQuery query);
    Task<DockScheduleItemResponse> GetByIdAsync(Guid id);
    Task<DockScheduleItemResponse> CreateAsync(CreateDockScheduleRequest request);
    Task<DockScheduleItemResponse> UpdateAsync(Guid id, UpdateDockScheduleRequest request);
    Task DeleteAsync(Guid id);
}
