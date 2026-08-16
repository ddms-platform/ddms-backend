using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Dock;

/// <summary>
/// Khoang neo truoc day khong duoc luu o dau ca — ca hai man deu suy ra tu VI
/// TRI TRONG MANG. Admin duyet toan bo tau tai ben nen tau thu 8 ra A12; trang
/// owner loc rieng tau cua minh nen chinh tau do thanh A1. Khoang con tu doi
/// moi khi co tau khac vao hoac roi ben.
///
/// Bo test nay chot hanh vi cua khoang neo THAT: luu vao dock_schedule, kiem
/// tra ton tai tren so do, nam trong suc chua ben, va khong hai tau cung mot
/// khoang trong cung khoang thoi gian.
/// </summary>
public class AssignBerthTests
{
    private static readonly Guid LichNeoId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BenId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly DateTime BatDau = new(2026, 8, 16, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime KetThuc = new(2026, 8, 16, 18, 0, 0, DateTimeKind.Utc);

    private static (DDMS.Backend.Services.Implementations.DockScheduleService Service,
                    dock_schedule LichNeo,
                    Mock<IDockScheduleRepository> Repo)
        Build(int sucChuaBen = 50, bool khoangDaCoTauKhac = false)
    {
        var repo = new Mock<IDockScheduleRepository>();

        var lichNeo = new dock_schedule
        {
            id = LichNeoId,
            dock_id = BenId,
            boat_id = Guid.NewGuid(),
            start_time = BatDau,
            end_time = KetThuc,
        };

        repo.Setup(r => r.GetByIdAsync(LichNeoId)).ReturnsAsync(lichNeo);
        repo.Setup(r => r.GetDockAsync(BenId))
            .ReturnsAsync(new dock { id = BenId, name = "Ben thu nghiem", max_boats = sucChuaBen });
        repo.Setup(r => r.HasBerthConflictAsync(
                BenId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()))
            .ReturnsAsync(khoangDaCoTauKhac);

        var service = new DDMS.Backend.Services.Implementations.DockScheduleService(repo.Object);
        return (service, lichNeo, repo);
    }

    [Fact]
    public async Task GanKhoangHopLe_LuuVaoLichNeo()
    {
        var (service, lichNeo, repo) = Build();

        var ketQua = await service.AssignBerthAsync(LichNeoId, "A12", CancellationToken.None);

        lichNeo.berth_code.Should().Be("A12");
        ketQua.berthCode.Should().Be("A12");
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChuanHoaChuThuongVaKhoangTrang()
    {
        var (service, lichNeo, _) = Build();

        await service.AssignBerthAsync(LichNeoId, "  a12 ", CancellationToken.None);

        lichNeo.berth_code.Should().Be("A12");
    }

    [Theory]
    [InlineData("A99")]
    [InlineData("C1")]
    [InlineData("khoang A12")]
    [InlineData("A0")]
    public async Task KhoangKhongCoTrenSoDo_Nem(string khoang)
    {
        var (service, lichNeo, repo) = Build();

        var act = async () => await service.AssignBerthAsync(LichNeoId, khoang, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        lichNeo.berth_code.Should().BeNull();
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Bến 4 chỗ chỉ mở 4 khoang đầu của DockBerths.Ordered = A1, A9, A2, A10.
    /// A12 nằm ngoài, không gán được.
    /// </summary>
    [Fact]
    public async Task KhoangVuotSucChuaBen_Nem()
    {
        var (service, lichNeo, _) = Build(sucChuaBen: 4);

        var act = async () => await service.AssignBerthAsync(LichNeoId, "A12", CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        lichNeo.berth_code.Should().BeNull();
    }

    [Fact]
    public async Task KhoangDaCoTauKhacTrongCungKhoangThoiGian_Nem()
    {
        var (service, lichNeo, repo) = Build(khoangDaCoTauKhac: true);

        var act = async () => await service.AssignBerthAsync(LichNeoId, "A12", CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        lichNeo.berth_code.Should().BeNull();
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>Bỏ trống để gỡ khoang khỏi lịch neo.</summary>
    [Fact]
    public async Task GanChuoiRong_GoKhoang()
    {
        var (service, lichNeo, _) = Build();
        lichNeo.berth_code = "A12";

        await service.AssignBerthAsync(LichNeoId, "", CancellationToken.None);

        lichNeo.berth_code.Should().BeNull();
    }

    [Fact]
    public async Task LichNeoKhongTonTai_Nem()
    {
        var (service, _, repo) = Build();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((dock_schedule?)null);

        var act = async () => await service.AssignBerthAsync(Guid.NewGuid(), "A1", CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    /// <summary>
    /// Thứ tự khoang phải khớp với sơ đồ vẽ ở frontend — hai hàng xen kẽ nhau.
    /// Chính thứ tự này giải thích vì sao tàu thứ 8 hiện ra "A12".
    /// </summary>
    [Fact]
    public void ThuTuKhoangKhopVoiSoDoBen()
    {
        DockBerths.Ordered.Take(9).Should()
            .Equal("A1", "A9", "A2", "A10", "A3", "A11", "A4", "A12", "A5");
    }
}
