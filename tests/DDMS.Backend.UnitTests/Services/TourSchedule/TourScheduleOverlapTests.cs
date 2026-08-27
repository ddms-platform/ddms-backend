using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Models.DTOs.TourSchedule;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Implementations;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.TourSchedule;

public class TourScheduleOverlapTests
{
    private static readonly Guid OwnerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherOwnerId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid TourId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid BoatId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static (TourScheduleService Service, Mock<ITourScheduleRepository> Repo)
        Build(Guid userId, bool isAdmin = false)
    {
        var repo = new Mock<ITourScheduleRepository>();
        var user = new Mock<ICurrentUser>();
        user.SetupGet(u => u.Id).Returns(userId);
        user.Setup(u => u.IsInRole(RoleNames.Admin)).Returns(isAdmin);

        repo.Setup(r => r.ExistsTourAsync(TourId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.ExistsBoatAsync(BoatId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetBoatOwnerIdAsync(BoatId, It.IsAny<CancellationToken>())).ReturnsAsync(OwnerId);
        repo.Setup(r => r.GetTourCreatedByAsync(TourId, It.IsAny<CancellationToken>())).ReturnsAsync(OwnerId);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.AddAsync(It.IsAny<tour_schedule>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return (new TourScheduleService(repo.Object, user.Object), repo);
    }

    private static CreateTourScheduleRequest Request() => new()
    {
        tour_id = TourId,
        boat_id = BoatId,
        start_time = new DateTime(2026, 8, 27, 14, 25, 0, DateTimeKind.Utc),
        end_time = new DateTime(2026, 8, 27, 16, 25, 0, DateTimeKind.Utc),
        status = TourScheduleStatuses.Scheduled,
    };

    [Fact]
    public async Task Create_TrungGioCungThuyen_Nem()
    {
        var (service, repo) = Build(OwnerId);
        repo.Setup(r => r.HasBoatScheduleOverlapAsync(
                BoatId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await service.CreateAsync(Request(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be(ErrorCode.ScheduleBoatOverlap);
        repo.Verify(r => r.AddAsync(It.IsAny<tour_schedule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_ThuyenNguoiKhac_NemForbidden()
    {
        var (service, repo) = Build(OtherOwnerId);

        var act = async () => await service.CreateAsync(Request(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be(ErrorCode.Forbidden);
        repo.Verify(r => r.AddAsync(It.IsAny<tour_schedule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_KhongTrung_VanTaoDuoc()
    {
        var (service, repo) = Build(OwnerId);
        repo.Setup(r => r.HasBoatScheduleOverlapAsync(
                BoatId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(Request(), CancellationToken.None);

        result.tour_id.Should().Be(TourId);
        result.boat_id.Should().Be(BoatId);
        repo.Verify(r => r.AddAsync(It.IsAny<tour_schedule>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
