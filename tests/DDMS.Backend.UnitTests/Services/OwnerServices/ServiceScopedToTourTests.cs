using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.OwnerServices;

/// <summary>
/// Phòng và combo trước đây gắn theo con thuyền chứ không theo tour, và lúc cập
/// nhật thì gọi RemoveCabinsByBoatIdAsync — xoá phòng của MỌI tour chạy trên
/// cùng con thuyền. Chủ thuyền sửa tour A là tour B mất sạch hạng phòng.
///
/// serviceType cũng bị vứt: client gửi lên nhưng không có chỗ nào lưu, nên mở
/// lại form thì dịch vụ nào cũng thành "cruise".
/// </summary>
public class ServiceScopedToTourTests
{
    private static readonly Guid ChuThuyen = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ThuyenId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid TourDangSua = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    private static (DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService Service,
                    Mock<IOwnerServicesRegistrationRepository> Repo,
                    List<boat_cabin> CabinDaThem,
                    List<boat_service> ComboDaThem,
                    tour TourGoc)
        Build()
    {
        var tours = new Mock<ITourService>();
        var repo = new Mock<IOwnerServicesRegistrationRepository>();
        var boats = new Mock<IBoatRepository>();
        var docs = new Mock<IOwnerDocumentService>();
        var email = new Mock<IEmailSender>();

        boats.Setup(b => b.GetByIdAsync(ThuyenId))
            .ReturnsAsync(new boat { id = ThuyenId, owner_id = ChuThuyen });
        docs.Setup(d => d.GetOverviewByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DDMS.Backend.Models.DTOs.OwnerDocument.OwnerDocumentsOverviewResponse());

        var tourGoc = new tour { id = TourDangSua, name = "Tour cu", status = "pending" };
        repo.Setup(r => r.FindTourByIdAsync(TourDangSua, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tourGoc);

        var cabins = new List<boat_cabin>();
        var combos = new List<boat_service>();
        repo.Setup(r => r.AddBoatCabin(It.IsAny<boat_cabin>())).Callback<boat_cabin>(cabins.Add);
        repo.Setup(r => r.AddBoatService(It.IsAny<boat_service>())).Callback<boat_service>(combos.Add);

        tours.Setup(t => t.CreateAsync(It.IsAny<CreateTourRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TourResponse { id = Guid.NewGuid(), name = "Tour moi" });

        var service = new DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService(
            tours.Object, repo.Object, boats.Object, docs.Object, email.Object);

        return (service, repo, cabins, combos, tourGoc);
    }

    private static DynamicServiceRequest YeuCau(Guid? tourId = null) => new()
    {
        id = tourId,
        boatId = ThuyenId,
        name = "Tour hai san",
        basePrice = 2_000_000m,
        serviceType = "dinner",
        rooms = [new ServiceRoom { name = "Phong VIP", capacity = 2, price = 500_000m }],
        combos = [new ServiceCombo { name = "Combo hai san", price = 300_000m }],
    };

    /// <summary>Đây là ca đang hỏng: xoá theo tàu thì tour khác mất phòng.</summary>
    [Fact]
    public async Task CapNhat_XoaPhongTheoTour_KhongXoaTheoTau()
    {
        var (service, repo, _, _, _) = Build();

        await service.RegisterAsync(YeuCau(TourDangSua), ChuThuyen, CancellationToken.None);

        repo.Verify(r => r.RemoveCabinsByTourIdAsync(TourDangSua, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.RemoveCombosByTourIdAsync(TourDangSua, It.IsAny<CancellationToken>()), Times.Once);

        repo.Verify(r => r.RemoveCabinsByBoatIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.RemoveCombosByBoatIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CapNhat_PhongVaComboDuocGanVaoTour()
    {
        var (service, _, cabins, combos, _) = Build();

        await service.RegisterAsync(YeuCau(TourDangSua), ChuThuyen, CancellationToken.None);

        cabins.Should().OnlyContain(c => c.tour_id == TourDangSua && c.boat_id == ThuyenId);
        combos.Should().OnlyContain(c => c.tour_id == TourDangSua && c.boat_id == ThuyenId);
    }

    [Fact]
    public async Task TaoMoi_PhongVaComboDuocGanVaoTourVuaTao()
    {
        var (service, _, cabins, combos, _) = Build();

        var ketQua = await service.RegisterAsync(YeuCau(), ChuThuyen, CancellationToken.None);

        cabins.Should().OnlyContain(c => c.tour_id == ketQua.id);
        combos.Should().OnlyContain(c => c.tour_id == ketQua.id);
    }

    [Fact]
    public async Task CapNhat_LuuServiceType()
    {
        var (service, _, _, _, tourGoc) = Build();

        await service.RegisterAsync(YeuCau(TourDangSua), ChuThuyen, CancellationToken.None);

        tourGoc.service_type.Should().Be("dinner");
    }

    [Fact]
    public async Task TaoMoi_GuiServiceTypeSangTourService()
    {
        var tours = new Mock<ITourService>();
        var repo = new Mock<IOwnerServicesRegistrationRepository>();
        var boats = new Mock<IBoatRepository>();
        var docs = new Mock<IOwnerDocumentService>();

        boats.Setup(b => b.GetByIdAsync(ThuyenId))
            .ReturnsAsync(new boat { id = ThuyenId, owner_id = ChuThuyen });
        docs.Setup(d => d.GetOverviewByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DDMS.Backend.Models.DTOs.OwnerDocument.OwnerDocumentsOverviewResponse());

        CreateTourRequest? daGui = null;
        tours.Setup(t => t.CreateAsync(It.IsAny<CreateTourRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTourRequest, CancellationToken>((r, _) => daGui = r)
            .ReturnsAsync(new TourResponse { id = Guid.NewGuid(), name = "Tour moi" });

        var service = new DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService(
            tours.Object, repo.Object, boats.Object, docs.Object, new Mock<IEmailSender>().Object);

        await service.RegisterAsync(YeuCau(), ChuThuyen, CancellationToken.None);

        daGui.Should().NotBeNull();
        daGui!.service_type.Should().Be("dinner");
    }

    [Fact]
    public async Task TaoMoi_FishingKhongCoPhongCombo_VanGanTourVaoThuyen()
    {
        var (service, _, cabins, combos, _) = Build();

        var ketQua = await service.RegisterAsync(
            new DynamicServiceRequest
            {
                boatId = ThuyenId,
                name = "Cau ca dem",
                basePrice = 1_000_000m,
                serviceType = "fishing",
            },
            ChuThuyen,
            CancellationToken.None);

        cabins.Should().BeEmpty();
        combos.Should().ContainSingle(c =>
            c.tour_id == ketQua.id && c.boat_id == ThuyenId && c.name == "Cau ca dem");
    }

    [Fact]
    public async Task CapNhat_TourDangBan_KhongGhiDe_KhongTaoTourMoi()
    {
        var (service, repo, cabins, combos, tourGoc) = Build();
        tourGoc.status = "active";
        service_change_request? phieu = null;
        repo.Setup(r => r.AddChangeRequest(It.IsAny<service_change_request>()))
            .Callback<service_change_request>(x => phieu = x);

        var ketQua = await service.RegisterAsync(YeuCau(TourDangSua), ChuThuyen, CancellationToken.None);

        ketQua.id.Should().Be(TourDangSua);
        ketQua.status.Should().Be("active");
        ketQua.approvalKind.Should().Be("service_change");
        tourGoc.name.Should().Be("Tour cu");
        cabins.Should().BeEmpty();
        combos.Should().BeEmpty();
        phieu.Should().NotBeNull();
        phieu!.tour_id.Should().Be(TourDangSua);
        repo.Verify(r => r.RemoveCabinsByTourIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(
            r => r.AddChangeRequest(It.IsAny<service_change_request>()),
            Times.Once);
    }

    [Fact]
    public async Task CapNhat_IdKhongTonTai_KhongTaoTourMoi()
    {
        var (service, _, _, _, _) = Build();

        var act = async () => await service.RegisterAsync(
            YeuCau(Guid.NewGuid()), ChuThuyen, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
