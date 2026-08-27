using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.OwnerServices;

/// <summary>
/// POST /api/owner/services/register truoc day khong co [Authorize] ("tat de de
/// test"), va service chi lay owner_id TU CON THUYEN chu khong doi chieu voi
/// nguoi goi. Ai biet boatId cung tao duoc tour tren thuyen do.
///
/// Bat [Authorize] moi chi chan nguoi la. Ranh buoc so huu moi chan duoc chu
/// thuyen A dung vao thuyen cua chu thuyen B.
/// </summary>
public class RegisterServiceOwnershipTests
{
    private static readonly Guid ChuThuyenA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ChuThuyenB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid ThuyenCuaA = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static (DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService Service,
                    Mock<ITourService> Tours,
                    Mock<IOwnerServicesRegistrationRepository> Repo)
        Build()
    {
        var tours = new Mock<ITourService>();
        var repo = new Mock<IOwnerServicesRegistrationRepository>();
        var boats = new Mock<IBoatRepository>();
        var docs = new Mock<IOwnerDocumentService>();
        var email = new Mock<IEmailSender>();

        boats.Setup(b => b.GetByIdAsync(ThuyenCuaA))
            .ReturnsAsync(new boat { id = ThuyenCuaA, owner_id = ChuThuyenA, name = "Thuyen cua A" });

        docs.Setup(d => d.GetOverviewByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DDMS.Backend.Models.DTOs.OwnerDocument.OwnerDocumentsOverviewResponse());

        var service = new DDMS.Backend.Services.Implementations.OwnerServicesRegistrationService(
            tours.Object, repo.Object, boats.Object, docs.Object, email.Object);

        return (service, tours, repo);
    }

    private static DynamicServiceRequest YeuCau() => new()
    {
        boatId = ThuyenCuaA,
        name = "Tour thu nghiem",
        basePrice = 1_000_000m,
    };

    /// <summary>Đây là ca đang hổng: B đăng ký dịch vụ trên thuyền của A.</summary>
    [Fact]
    public async Task ThuyenCuaNguoiKhac_Nem_KhongTaoTour()
    {
        var (service, tours, repo) = Build();

        var act = async () => await service.RegisterAsync(YeuCau(), ChuThuyenB, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();

        tours.Verify(t => t.CreateAsync(It.IsAny<DDMS.Backend.Models.DTOs.Tour.CreateTourRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ThuyenKhongTonTai_Nem_KhongTaoTour()
    {
        var (service, tours, repo) = Build();
        var yeuCau = YeuCau();
        yeuCau.boatId = Guid.NewGuid();

        var act = async () => await service.RegisterAsync(yeuCau, ChuThuyenA, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();

        tours.Verify(t => t.CreateAsync(It.IsAny<DDMS.Backend.Models.DTOs.Tour.CreateTourRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>Chủ thuyền thật vẫn phải đăng ký được — không chặn nhầm.</summary>
    [Fact]
    public async Task ChinhChuThuyen_VanTaoDuocTour()
    {
        var (service, tours, repo) = Build();
        tours.Setup(t => t.CreateAsync(It.IsAny<DDMS.Backend.Models.DTOs.Tour.CreateTourRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DDMS.Backend.Models.DTOs.Tour.TourResponse { id = Guid.NewGuid(), name = "Tour thu nghiem" });

        await service.RegisterAsync(YeuCau(), ChuThuyenA, CancellationToken.None);

        tours.Verify(t => t.CreateAsync(It.IsAny<DDMS.Backend.Models.DTOs.Tour.CreateTourRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
