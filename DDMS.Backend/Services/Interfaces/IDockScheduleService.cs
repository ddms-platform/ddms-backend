using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;


namespace DDMS.Backend.Services.Interfaces;

public interface IDockScheduleService
{
    Task<PagedResponse<DockScheduleItemResponse>> GetSchedulesAsync(DockScheduleListQuery query);
    Task<DockScheduleItemResponse> GetByIdAsync(Guid id);
    Task<DockScheduleItemResponse> CreateAsync(CreateDockScheduleRequest request);
    Task<DockScheduleItemResponse> UpdateAsync(Guid id, UpdateDockScheduleRequest request);

    /// <summary>
    /// Cang vu gan khoang neo cho mot lich neo. Truyen chuoi rong de go khoang.
    /// </summary>
    Task<Models.DTOs.Dock.BerthAssignmentResponse> AssignBerthAsync(Guid dockScheduleId, string? berthCode, CancellationToken ct);
    Task DeleteAsync(Guid id);
}
