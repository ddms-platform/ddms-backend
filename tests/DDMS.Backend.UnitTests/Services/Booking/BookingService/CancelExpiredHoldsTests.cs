using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// BookingService.CancelExpiredHoldsAsync chỉ là 1 lệnh ủy quyền thẳng xuống repository (không có nhánh rẽ),
/// nên chỉ cần 1 case xác nhận tham số truyền xuống đúng và giá trị trả về được chuyển tiếp nguyên vẹn.
/// </summary>
public class CancelExpiredHoldsTests
{
    [Fact]
    public async Task CancelExpiredHoldsAsync_DelegatesToRepositoryAndReturnsCount()
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        bookingRepo.Setup(r => r.CancelExpiredHoldsAsync(
                It.IsAny<DateTime>(), "Tự động huỷ do quá hạn giữ chỗ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions,
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object);

        var result = await service.CancelExpiredHoldsAsync(CancellationToken.None);

        result.Should().Be(3);
    }
}
