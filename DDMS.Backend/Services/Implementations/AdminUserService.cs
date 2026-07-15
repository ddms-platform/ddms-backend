using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Admin.Users;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class AdminUserService : IAdminUserService
{
    private static readonly HashSet<string> AllowedRoles = ["admin", "owner", "user", "agent"];

    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AdminUserService(
        IAdminUserRepository adminUserRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _adminUserRepository = adminUserRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<PagedResponse<AdminUserListItemResponse>> GetUsersAsync(AdminUserListQuery query)
    {
        var (items, total) = await _adminUserRepository.GetPagedAsync(query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        return new PagedResponse<AdminUserListItemResponse>
        {
            items = items.Select(MapToListItem).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public Task<AdminUserStatsResponse> GetStatsAsync()
    {
        return _adminUserRepository.GetStatsAsync();
    }

    public async Task<AdminUserListItemResponse> GetByIdAsync(Guid id)
    {
        var user = await _adminUserRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException();
        }

        return MapToListItem(user);
    }

    public async Task<AdminUserListItemResponse> UpdateAsync(
        Guid id,
        UpdateAdminUserRequest request,
        Guid currentUserId)
    {
        EnsureNotSelf(id, currentUserId);

        var user = await _adminUserRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException();
        }

        if (string.IsNullOrWhiteSpace(request.fullName))
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["fullName"] = [ErrorCode.Messages.FullNameRequired]
            });
        }

        var wasActive = user.is_active ?? true;

        user.full_name = request.fullName.Trim();
        user.phone = string.IsNullOrWhiteSpace(request.phone) ? null : request.phone.Trim();
        user.is_active = request.isActive;

        await _userRepository.UpdateAsync(user);

        if (wasActive && request.isActive == false)
        {
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(id);
        }
        return MapToListItem(user);
    }

    public async Task<AdminUserListItemResponse> UpdateRolesAsync(
        Guid id,
        UpdateAdminUserRolesRequest request,
        Guid currentUserId)
    {
        EnsureNotSelf(id, currentUserId);

        var user = await _adminUserRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException();
        }

        if (request.roles.Count == 0)
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["roles"] = [ErrorCode.Messages.InvalidRole]
            });
        }

        var normalizedRoles = request.roles
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (normalizedRoles.Any(x => !AllowedRoles.Contains(x)))
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.InvalidRole);
        }

        var roles = await _roleRepository.GetByNamesAsync(normalizedRoles);
        if (roles.Count != normalizedRoles.Count)
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.InvalidRole);
        }

        await _adminUserRepository.SetRolesAsync(id, roles.Select(x => x.id));
        var updated = await _adminUserRepository.GetByIdAsync(id);
        return MapToListItem(updated!);
    }

    public async Task DeactivateAsync(Guid id, Guid currentUserId)
    {
        EnsureNotSelf(id, currentUserId);

        var user = await _adminUserRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new NotFoundException();
        }

        user.is_active = false;
        await _userRepository.UpdateAsync(user);
        await _refreshTokenRepository.RevokeAllActiveForUserAsync(id);
    }

    private static void EnsureNotSelf(Guid targetUserId, Guid currentUserId)
    {
        if (targetUserId == currentUserId)
        {
            throw new ForbiddenException(ErrorCode.Messages.CannotModifySelf);
        }
    }

    private static AdminUserListItemResponse MapToListItem(user user)
    {
        return new AdminUserListItemResponse
        {
            id = user.id,
            fullName = user.full_name,
            email = user.email,
            phone = user.phone,
            roles = user.user_roles.Select(x => x.role.name).OrderBy(x => x).ToList(),
            isActive = user.is_active ?? true,
            emailVerified = user.email_verified_at is not null,
            ownerVerified = user.owner_profile?.is_verified ?? false,
            createdAt = user.created_at
        };
    }
}
