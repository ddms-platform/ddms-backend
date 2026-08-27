using System.Reflection;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace DDMS.Backend.UnitTests.Controllers;

/// <summary>
/// POST/PUT/DELETE /api/tour-schedules trước đây không có [Authorize].
/// GET theo tour phải giữ mở vì trang chi tiết/đặt chỗ của khách đang gọi.
/// </summary>
public class TourScheduleAuthorizationTests
{
    [Fact]
    public void Create_ChiOwnerHoacAdmin()
    {
        AssertOwnerOrAdmin(nameof(TourScheduleController.Create));
    }

    [Fact]
    public void Update_ChiOwnerHoacAdmin()
    {
        AssertOwnerOrAdmin(nameof(TourScheduleController.Update));
    }

    [Fact]
    public void Delete_ChiOwnerHoacAdmin()
    {
        AssertOwnerOrAdmin(nameof(TourScheduleController.Delete));
    }

    [Fact]
    public void GetByTourId_VanMoChoKhachVangLai()
    {
        var method = typeof(TourScheduleController).GetMethod(nameof(TourScheduleController.GetByTourId))!;
        method.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
        method.GetCustomAttribute<AuthorizeAttribute>().Should().BeNull();
        typeof(TourScheduleController).GetCustomAttribute<AuthorizeAttribute>().Should().BeNull();
    }

    private static void AssertOwnerOrAdmin(string methodName)
    {
        var method = typeof(TourScheduleController).GetMethod(methodName)!;
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        authorize.Should().NotBeNull();
        authorize!.Roles.Should().Be($"{RoleNames.Owner},{RoleNames.Admin}");
        method.GetCustomAttribute<AllowAnonymousAttribute>().Should().BeNull();
    }
}
