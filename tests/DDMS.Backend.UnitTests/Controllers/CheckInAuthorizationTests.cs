using System.Reflection;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace DDMS.Backend.UnitTests.Controllers;

/// <summary>
/// Check-in đổi trạng thái vé của người khác, và mã vé chỉ là 8 ký tự đầu của GUID
/// (tra bằng LIKE 'prefix%'), nên endpoint này tuyệt đối không được để ẩn danh —
/// ai cũng gọi được thì ai cũng phá được vé của khách.
/// </summary>
public class CheckInAuthorizationTests
{
    private static MethodInfo CheckInAction =>
        typeof(PublicToursController).GetMethod(nameof(PublicToursController.CheckInBooking))!;

    [Fact]
    public void CheckInBooking_YeuCauDangNhap()
    {
        CheckInAction.GetCustomAttribute<AllowAnonymousAttribute>().Should().BeNull();
        CheckInAction.GetCustomAttribute<AuthorizeAttribute>().Should().NotBeNull();
    }

    [Fact]
    public void CheckInBooking_ChiDanhChoAdmin()
    {
        var authorize = CheckInAction.GetCustomAttribute<AuthorizeAttribute>();

        authorize!.Roles.Should().Be(RoleNames.Admin);
    }

    [Fact]
    public void CacEndpointTraCuuTourVanMoChoKhachVangLai()
    {
        // Đừng khoá nhầm cả controller: danh mục tour phải xem được khi chưa đăng nhập.
        var getById = typeof(PublicToursController).GetMethod(nameof(PublicToursController.GetById))!;

        getById.GetCustomAttribute<AuthorizeAttribute>().Should().BeNull();
        typeof(PublicToursController).GetCustomAttribute<AuthorizeAttribute>().Should().BeNull();
    }
}
