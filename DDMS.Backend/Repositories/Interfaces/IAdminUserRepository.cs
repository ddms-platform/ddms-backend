using DDMS.Backend.Models.DTOs.Admin.Users;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IAdminUserRepository
{
    Task<(List<user> items, int total)> GetPagedAsync(AdminUserListQuery query);
    Task<AdminUserStatsResponse> GetStatsAsync();
    Task<user?> GetByIdAsync(Guid id);
    Task SetRolesAsync(Guid userId, IEnumerable<int> roleIds);
}
