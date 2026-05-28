using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Admin.Users;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IAdminUserService
{
    Task<PagedResponse<AdminUserListItemResponse>> GetUsersAsync(AdminUserListQuery query);
    Task<AdminUserStatsResponse> GetStatsAsync();
    Task<AdminUserListItemResponse> GetByIdAsync(Guid id);
    Task<AdminUserListItemResponse> UpdateAsync(Guid id, UpdateAdminUserRequest request, Guid currentUserId);
    Task<AdminUserListItemResponse> UpdateRolesAsync(Guid id, UpdateAdminUserRolesRequest request, Guid currentUserId);
    Task DeactivateAsync(Guid id, Guid currentUserId);
}
