using DDMS.Backend.Common.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Chuyển đơn sang "completed" sau khi chuyến đã chạy xong.
///
/// Trước đây không chỗ nào trong toàn hệ thống ghi trạng thái này, mà
/// ReviewRepository lại lọc đúng nó — nên không khách nào từng đánh giá được tour.
/// </summary>
public class CompleteFinishedToursTests
{
    private readonly Mock<DDMS.Backend.Repositories.Interfaces.IBookingRepository> _bookingRepo =
        BookingRepositoryMockFactory.Create();

    private DDMS.Backend.Services.Implementations.BookingService CreateSut() =>
        new(
            _bookingRepo.Object,
            WalletRepositoryMockFactory.Create().Object,
            EmailSenderMockFactory.Create().Object,
            NotificationServiceMockFactory.Create().Object,
            OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>(),
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object,
            BookingPaymentRepositoryMockFactory.Create().Object);

    [Fact]
    public async Task GoiXuongRepoVoiMocThoiGianHienTai()
    {
        var before = DateTime.UtcNow;
        _bookingRepo.Setup(r => r.CompleteFinishedToursAsync(
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var completed = await CreateSut().CompleteFinishedToursAsync(CancellationToken.None);

        completed.Should().Be(3);
        _bookingRepo.Verify(r => r.CompleteFinishedToursAsync(
            It.Is<DateTime>(now => now >= before && now <= DateTime.UtcNow),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ChiDonDaTraTienMoiDuocChuyenSangHoanThanh()
    {
        // Đơn huỷ hoặc chưa thanh toán thì chuyến có chạy xong cũng không thành
        // "đã hoàn thành" — khách đâu có đi.
        BookingStatuses.CompletableStatuses.Should().BeEquivalentTo(new[]
        {
            BookingStatuses.Confirmed,
            BookingStatuses.Paid,
            BookingStatuses.CheckedIn,
        });
    }

    [Fact]
    public void MoiTrangThaiChuyenDuocSangHoanThanhDeuLaDaTraTien()
    {
        // Khoá lại quan hệ giữa hai khái niệm: nếu sau này ai thêm một trạng thái
        // vào CompletableStatuses mà nó không phải "đã trả tiền", test này gãy.
        foreach (var status in BookingStatuses.CompletableStatuses)
            BookingStatuses.IsPaidLike(status).Should().BeTrue($"trạng thái '{status}'");
    }
}
