using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace DDMS.Backend.UnitTests.Services.OwnerServices;

public class ServiceChangeRequestTests
{
    private static readonly Guid ChuThuyen = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ThuyenId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid TourId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid PhieuId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    private static DynamicServiceRequest Payload() => new()
    {
        id = TourId,
        boatId = ThuyenId,
        name = "Tour da sua",
        basePrice = 3_000_000m,
        serviceType = "dinner",
        combos = [new ServiceCombo { name = "Combo moi", price = 200_000m }],
    };

    [Fact]
    public async Task DuyetPhieu_ApLenDungTour_KhongTaoTourMoi()
    {
        var tours = new Mock<ITourService>();
        var repo = new Mock<IOwnerServicesRegistrationRepository>();
        var boats = new Mock<IBoatRepository>();
        var docs = new Mock<IOwnerDocumentService>();
        var tour = new tour { id = TourId, name = "Tour cu", status = "active", price = 1_000_000m };
        var phieu = new service_change_request
        {
            id = PhieuId,
            tour_id = TourId,
            boat_id = ThuyenId,
            owner_id = ChuThuyen,
            status = "pending",
            payload_json = JsonSerializer.Serialize(Payload(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }),
            tour = tour,
        };

        repo.Setup(r => r.FindChangeByIdAsync(PhieuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phieu);

        var service = new DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService(
            tours.Object, repo.Object, boats.Object, docs.Object, new Mock<IEmailSender>().Object);

        var ketQua = await service.ApproveChangeAsync(PhieuId, CancellationToken.None);

        ketQua.status.Should().Be("approved");
        tour.id.Should().Be(TourId);
        tour.name.Should().Be("Tour da sua");
        tour.status.Should().Be("active");
        tours.Verify(t => t.CreateAsync(It.IsAny<CreateTourRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TuChoiPhieu_TourLiveKhongDoi()
    {
        var repo = new Mock<IOwnerServicesRegistrationRepository>();
        var tour = new tour { id = TourId, name = "Tour cu", status = "active", price = 1_000_000m };
        var phieu = new service_change_request
        {
            id = PhieuId,
            tour_id = TourId,
            status = "pending",
            payload_json = "{}",
            tour = tour,
        };
        repo.Setup(r => r.FindChangeByIdAsync(PhieuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phieu);

        var service = new DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService(
            new Mock<ITourService>().Object,
            repo.Object,
            new Mock<IBoatRepository>().Object,
            new Mock<IOwnerDocumentService>().Object,
            new Mock<IEmailSender>().Object);

        await service.RejectChangeAsync(PhieuId, "Thieu anh", CancellationToken.None);

        phieu.status.Should().Be("rejected");
        phieu.rejection_reason.Should().Be("Thieu anh");
        tour.name.Should().Be("Tour cu");
        tour.status.Should().Be("active");
    }

    [Fact]
    public async Task DuyetPhieuDaXuLy_Nem()
    {
        var repo = new Mock<IOwnerServicesRegistrationRepository>();
        repo.Setup(r => r.FindChangeByIdAsync(PhieuId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new service_change_request { id = PhieuId, status = "approved" });

        var service = new DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService(
            new Mock<ITourService>().Object,
            repo.Object,
            new Mock<IBoatRepository>().Object,
            new Mock<IOwnerDocumentService>().Object,
            new Mock<IEmailSender>().Object);

        var act = async () => await service.ApproveChangeAsync(PhieuId, CancellationToken.None);
        await act.Should().ThrowAsync<AppException>();
    }
}
