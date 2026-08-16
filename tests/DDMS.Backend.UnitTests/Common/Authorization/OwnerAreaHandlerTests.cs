using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Authorization;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace DDMS.Backend.UnitTests.Common.Authorization;

/// <summary>
/// Vai trò "owner" chỉ được cấp khi cảng vụ duyệt hồ sơ. Trước đó chủ thuyền đã
/// khai thuyền rồi nhưng mọi API khu vực chủ thuyền trả 403, nên giao diện hiện
/// danh sách rỗng và mời họ đăng ký thuyền mới — dẫn tới thuyền trùng.
///
/// Chính sách này mở phần ĐỌC cho người đã nộp hồ sơ và đang chờ duyệt. Quyền
/// ghi vẫn đòi vai trò "owner" thật, khai báo riêng ở từng action.
/// </summary>
public class OwnerAreaHandlerTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static ClaimsPrincipal NguoiDung(params string[] roles)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, UserId.ToString()) };
        claims.AddRange(roles.Select(r => new Claim("role", r)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test", JwtRegisteredClaimNames.Sub, "role"));
    }

    private static async Task<bool> ChoQuaKhong(ClaimsPrincipal user, string? trangThaiHoSo)
    {
        var profiles = new Mock<IOwnerProfileRepository>();
        profiles.Setup(p => p.FindStatusByUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(trangThaiHoSo);

        var requirement = new OwnerAreaRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);

        await new OwnerAreaHandler(profiles.Object).HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task CoVaiTroOwner_ChoQua()
    {
        (await ChoQuaKhong(NguoiDung(RoleNames.User, RoleNames.Owner), trangThaiHoSo: null))
            .Should().BeTrue();
    }

    /// <summary>Đây là ca đang hỏng trên production.</summary>
    [Fact]
    public async Task ChuaCoVaiTro_NhungHoSoDangChoDuyet_ChoQua()
    {
        (await ChoQuaKhong(NguoiDung(RoleNames.User), OwnerProfileStatuses.Pending))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ChuaCoVaiTro_HoSoDaDuyet_ChoQua()
    {
        (await ChoQuaKhong(NguoiDung(RoleNames.User), OwnerProfileStatuses.Verified))
            .Should().BeTrue();
    }

    [Fact]
    public async Task HoSoBiTuChoi_Chan()
    {
        (await ChoQuaKhong(NguoiDung(RoleNames.User), OwnerProfileStatuses.Rejected))
            .Should().BeFalse();
    }

    [Fact]
    public async Task KhongCoHoSo_Chan()
    {
        (await ChoQuaKhong(NguoiDung(RoleNames.User), trangThaiHoSo: null))
            .Should().BeFalse();
    }

    /// <summary>
    /// Không có claim sub thì không tra được hồ sơ của ai — phải chặn, tuyệt đối
    /// không mặc định cho qua.
    /// </summary>
    [Fact]
    public async Task KhongCoClaimSub_Chan()
    {
        var khuyetDanh = new ClaimsPrincipal(new ClaimsIdentity([new Claim("role", RoleNames.User)], "test"));

        (await ChoQuaKhong(khuyetDanh, OwnerProfileStatuses.Verified)).Should().BeFalse();
    }
}
