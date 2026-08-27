using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Implementations;
using DDMS.Backend.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Tour;

public class TourRejectionReasonTests
{
    private static readonly Guid TourId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid OwnerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static (TourService Service, Mock<INotificationService> Notifications, tour Entity) Build()
    {
        var repo = new Mock<ITourRepository>();
        var notifications = new Mock<INotificationService>();
        var entity = new tour
        {
            id = TourId,
            name = "Tour thu nghiem",
            status = TourConstants.Statuses.Pending,
            created_by = OwnerId,
            cancel_policy = "free",
            price = 100_000,
            duration_minutes = 120,
        };

        repo.Setup(r => r.GetByIdAsync(TourId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        return (new TourService(repo.Object, notifications.Object), notifications, entity);
    }

    private static UpdateTourRequest Request(string status, string? reason = null) => new()
    {
        name = "Tour thu nghiem",
        price = 100_000,
        duration_minutes = 120,
        status = status,
        cancel_policy = "free",
        rejection_reason = reason,
    };

    [Fact]
    public async Task Reject_KhongCoLyDo_Nem()
    {
        var (service, _, _) = Build();

        var act = async () => await service.UpdateAsync(TourId, Request("rejected"), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Reject_CoLyDo_LuuVaThongBao()
    {
        var (service, notifications, entity) = Build();

        await service.UpdateAsync(TourId, Request("rejected", "Thieu anh thuyen"), CancellationToken.None);

        entity.status.Should().Be(TourConstants.Statuses.Rejected);
        entity.rejection_reason.Should().Be("Thieu anh thuyen");
        notifications.Verify(
            n => n.CreateNotificationAsync(
                null,
                "system",
                It.IsAny<string>(),
                It.Is<string>(b => b.Contains("Thieu anh thuyen")),
                It.IsAny<List<Guid>>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Approve_XoaLyDoTuChoiCu()
    {
        var (service, _, entity) = Build();
        entity.status = TourConstants.Statuses.Rejected;
        entity.rejection_reason = "Cu";

        await service.UpdateAsync(TourId, Request("active"), CancellationToken.None);

        entity.status.Should().Be(TourConstants.Statuses.Active);
        entity.rejection_reason.Should().BeNull();
    }
}
