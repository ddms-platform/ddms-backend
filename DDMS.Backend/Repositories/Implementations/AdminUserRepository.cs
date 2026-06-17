using DDMS.Backend.Models.DTOs.Admin.Users;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _dbContext;

    public AdminUserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<user> items, int total)> GetPagedAsync(AdminUserListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var usersQuery = _dbContext.users
            .Include(x => x.user_roles)
            .ThenInclude(x => x.role)
            .Include(x => x.owner_profile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.search))
        {
            var keyword = query.search.Trim().ToLowerInvariant();
            usersQuery = usersQuery.Where(x =>
                x.email.ToLower().Contains(keyword) ||
                x.full_name.ToLower().Contains(keyword) ||
                (x.phone != null && x.phone.ToLower().Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.role))
        {
            var roleName = query.role.Trim().ToLowerInvariant();
            usersQuery = usersQuery.Where(x =>
                x.user_roles.Any(ur => ur.role.name.ToLower() == roleName));
        }

        if (query.isActive.HasValue)
        {
            usersQuery = usersQuery.Where(x => x.is_active == query.isActive.Value);
        }

        if (query.emailVerified.HasValue)
        {
            usersQuery = query.emailVerified.Value
                ? usersQuery.Where(x => x.email_verified_at != null)
                : usersQuery.Where(x => x.email_verified_at == null);
        }

        var total = await usersQuery.CountAsync();
        var items = await usersQuery
            .OrderByDescending(x => x.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<AdminUserStatsResponse> GetStatsAsync()
    {
        var users = await _dbContext.users
            .Include(x => x.user_roles)
            .ThenInclude(x => x.role)
            .AsNoTracking()
            .ToListAsync();

        return new AdminUserStatsResponse
        {
            total = users.Count,
            adminCount = users.Count(x => x.user_roles.Any(r => r.role.name == "admin")),
            ownerCount = users.Count(x => x.user_roles.Any(r => r.role.name == "owner")),
            userCount = users.Count(x => x.user_roles.Any(r => r.role.name == "user"))
        };
    }

    public Task<user?> GetByIdAsync(Guid id)
    {
        return _dbContext.users
            .Include(x => x.user_roles)
            .ThenInclude(x => x.role)
            .Include(x => x.owner_profile)
            .FirstOrDefaultAsync(x => x.id == id);
    }

    public async Task SetRolesAsync(Guid userId, IEnumerable<int> roleIds)
    {
        var existing = await _dbContext.user_roles
            .Where(x => x.user_id == userId)
            .ToListAsync();

        _dbContext.user_roles.RemoveRange(existing);

        foreach (var roleId in roleIds.Distinct())
        {
            _dbContext.user_roles.Add(new user_role
            {
                user_id = userId,
                role_id = roleId,
                assigned_at = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
