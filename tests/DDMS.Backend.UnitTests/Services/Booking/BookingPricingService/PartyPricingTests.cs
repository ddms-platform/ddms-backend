using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingPricingService;

/// <summary>
/// Giá tour theo hạng vé. Người lớn luôn trả 100%; trẻ em và em bé trả theo tỉ lệ
/// owner khai báo trên từng tour. Tiền tour là phần lớn nhất của hoá đơn nên sai
/// một đồng ở đây là lệch khi đối chiếu với PayOS.
/// </summary>
public class PartyPricingTests
{
    private static readonly IReadOnlyCollection<BookingLineRequest> NoLines = [];

    private static DDMS.Backend.Services.Implementations.BookingPricingService Build(
        decimal tourPrice = 300_000m,
        decimal childPercent = 50m,
        decimal infantPercent = 0m)
    {
        var bookings = BookingRepositoryMockFactory.Create();
        var promotions = PromotionsRepositoryMockFactory.Create();

        var tour = new TourBuilder().WithPrice(tourPrice).Build();
        tour.child_price_percent = childPercent;
        tour.infant_price_percent = infantPercent;

        var schedule = new TourScheduleBuilder().WithTour(tour).Build();
        bookings.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        return new DDMS.Backend.Services.Implementations.BookingPricingService(
            bookings.Object, promotions.Object);
    }

    private static Task<BookingQuote> QuoteAsync(
        DDMS.Backend.Services.Implementations.BookingPricingService service, PartyComposition party) =>
        service.QuoteAsync(TestGuids.ScheduleId, party, NoLines, NoLines, null, CancellationToken.None);

    [Fact]
    public async Task ToanNguoiLon_TinhDuGia()
    {
        var quote = await QuoteAsync(Build(tourPrice: 300_000m), new PartyComposition(2, 0, 0));

        quote.BasePrice.Should().Be(600_000m);
    }

    [Fact]
    public async Task TreEm_TraNuaGia()
    {
        var quote = await QuoteAsync(Build(tourPrice: 300_000m), new PartyComposition(2, 1, 0));

        // 2 × 300.000 + 1 × 150.000
        quote.BasePrice.Should().Be(750_000m);
    }

    [Fact]
    public async Task EmBe_MienPhi()
    {
        var quote = await QuoteAsync(Build(tourPrice: 300_000m), new PartyComposition(1, 0, 2));

        quote.BasePrice.Should().Be(300_000m);
    }

    [Fact]
    public async Task ChiCoEmBe_ThiKhongMatTienTour()
    {
        var quote = await QuoteAsync(Build(tourPrice: 300_000m), new PartyComposition(0, 0, 3));

        quote.BasePrice.Should().Be(0m);
    }

    [Fact]
    public async Task TiLeOwnerTuKhaiBao_DuocTonTrong()
    {
        var service = Build(tourPrice: 300_000m, childPercent: 60m, infantPercent: 20m);

        var quote = await QuoteAsync(service, new PartyComposition(1, 1, 1));

        // 300.000 + 180.000 + 60.000
        quote.BasePrice.Should().Be(540_000m);
    }

    [Fact]
    public async Task LamTronTungDongChuKhongLamTronTong()
    {
        // 100.001 × 3 × 50% = 150.001,5 → làm tròn cả dòng thành 150.002.
        var service = Build(tourPrice: 100_001m, childPercent: 50m);

        var quote = await QuoteAsync(service, new PartyComposition(0, 3, 0));

        quote.BasePrice.Should().Be(150_002m);
    }

    [Fact]
    public async Task HoaDonTachRoTungHang()
    {
        var quote = await QuoteAsync(Build(tourPrice: 300_000m), new PartyComposition(2, 1, 1));

        quote.PartyLines.Should().HaveCount(3);
        quote.PartyLines.Should().ContainSingle(l => l.Tier == PassengerTiers.Adult && l.Quantity == 2
            && l.UnitPrice == 300_000m && l.LineTotal == 600_000m);
        quote.PartyLines.Should().ContainSingle(l => l.Tier == PassengerTiers.Child && l.Quantity == 1
            && l.UnitPrice == 150_000m);
        quote.PartyLines.Should().ContainSingle(l => l.Tier == PassengerTiers.Infant && l.Quantity == 1
            && l.UnitPrice == 0m);
    }

    [Fact]
    public async Task HangKhongCoKhach_ThiKhongHienTrenHoaDon()
    {
        var quote = await QuoteAsync(Build(), new PartyComposition(2, 0, 0));

        quote.PartyLines.Should().ContainSingle();
        quote.PartyLines[0].Tier.Should().Be(PassengerTiers.Adult);
    }
}
