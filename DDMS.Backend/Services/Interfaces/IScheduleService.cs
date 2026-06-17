using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Services.Interfaces;

public interface IScheduleService
{
    Task<PagedResponse<ScheduleItemResponse>> GetSchedulesAsync(Guid userId, ScheduleListQuery query);
    Task<ScheduleItemResponse> GetByIdAsync(Guid id, Guid userId);
    Task<ScheduleItemResponse> CreateAsync(Guid userId, CreateScheduleRequest request);
    Task<ScheduleItemResponse> UpdateAsync(Guid id, Guid userId, UpdateScheduleRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}
