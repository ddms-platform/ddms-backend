using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Số tiền báo cho khách phải đúng bằng số thực sự vào ví. Hoàn theo số PayOS
/// xác nhận đã nhận, không theo total_price — hai con số lệch nhau khi khách trả
/// thiếu hoặc khi đơn có khuyến mãi áp sau lúc thanh toán.
/// </summary>
public class RefundNotificationTests
{
    private readonly Mock<DDMS.Backend.Repositories.Interfaces.IBookingRepository> _bookingRepo =
        BookingRepositoryMockFactory.Create();
    private readonly Mock<DDMS.Backend.Repositories.Interfaces.IBookingPaymentRepository> _payments =
        BookingPaymentRepositoryMockFactory.Create();
    private readonly Mock<DDMS.Backend.Services.Interfaces.INotificationService> _notifications =
        NotificationServiceMockFactory.Create();

    private DDMS.Backend.Services.Implementations.BookingService CreateSut() =>
        new(
            _bookingRepo.Object,
            WalletRepositoryMockFactory.Create().Object,
            EmailSenderMockFactory.Create().Object,
            _notifications.Object,
            OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>(),
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object,
            _payments.Object);

    [Fact]
    public async Task ThongBaoHoanTien_BaoDungSoThucSuVaoVi()
    {
        // Đơn ghi 1.000.000 nhưng PayOS chỉ xác nhận nhận 700.000.
        var schedule = new TourScheduleBuilder().DepartingInDays(10).Build();
        var booking = new BookingBuilder()
            .WithSchedule(schedule)
            .WithStatus(BookingStatuses.Paid)
            .Build();
        booking.total_price = 1_000_000m;

        _bookingRepo.Setup(r => r.FindUserBookingWithScheduleAsync(
                TestGuids.BookingId, TestGuids.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _payments.Setup(p => p.HasAnyPaymentAsync(booking.id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _payments.Setup(p => p.GetPaidAmountAsync(booking.id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(700_000m);

        var result = await CreateSut().CancelAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.AmountRefunded.Should().Be(700_000m);

        // So theo chính định dạng mà code dùng: {:N0} đổi dấu phân cách theo culture của máy,
        // nên hành vi cần khóa là "đúng số tiền", không phải "đúng dấu chấm".
        var expected = 700_000m.ToString("N0");
        var wrong = 1_000_000m.ToString("N0");

        _notifications.Verify(n => n.CreateNotificationAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.Is<string>(title => title.Contains("Hoàn tiền")),
            It.Is<string>(body => body.Contains(expected) && !body.Contains(wrong)),
            It.IsAny<List<Guid>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
