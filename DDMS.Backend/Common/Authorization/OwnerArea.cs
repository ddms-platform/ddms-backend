using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DDMS.Backend.Common.Authorization;

public static class Policies
{
    /// <summary>
    /// Được vào khu vực chủ thuyền: đã có vai trò "owner", HOẶC đã nộp hồ sơ và
    /// đang chờ cảng vụ duyệt.
    /// </summary>
    public const string OwnerArea = "OwnerArea";
}

public sealed class OwnerAreaRequirement : IAuthorizationRequirement;

/// <summary>
/// Vai trò "owner" chỉ được cấp lúc cảng vụ duyệt hồ sơ (AdminOwnersService).
/// Nhưng thuyền đã được tạo ngay từ lúc nộp hồ sơ, nên trong khoảng chờ duyệt
/// chủ thuyền có thuyền trong DB mà mọi API khu vực chủ thuyền trả 403 — giao
/// diện hiện danh sách rỗng và mời họ khai lại một chiếc nữa.
///
/// Chính sách này mở phần ĐỌC cho hồ sơ đang chờ duyệt. Quyền GHI vẫn đòi vai
/// trò "owner" thật, khai báo riêng trên từng action.
/// </summary>
public sealed class OwnerAreaHandler : AuthorizationHandler<OwnerAreaRequirement>
{
    private static readonly string[] TrangThaiDuocVao =
    [
        OwnerProfileStatuses.Pending,
        OwnerProfileStatuses.Verified,
    ];

    private readonly IOwnerProfileRepository _profiles;

    public OwnerAreaHandler(IOwnerProfileRepository profiles) => _profiles = profiles;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OwnerAreaRequirement requirement)
    {
        if (context.User.IsInRole(RoleNames.Owner))
        {
            context.Succeed(requirement);
            return;
        }

        var raw = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Không đọc được id thì không tra được hồ sơ của ai. Chặn, không đoán.
        if (!Guid.TryParse(raw, out var userId))
            return;

        var status = await _profiles.FindStatusByUserAsync(userId, CancellationToken.None);

        if (status != null && TrangThaiDuocVao.Contains(status))
            context.Succeed(requirement);
    }
}
