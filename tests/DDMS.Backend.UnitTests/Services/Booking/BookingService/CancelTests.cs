using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Equivalence Partitioning cho BookingService.CancelAsync — xem TestData/Booking/BookingService/Cancel.json.
/// Lớp tương đương: không tìm thấy booking, đã huỷ trước đó, chưa thanh toán (không hoàn tiền),
/// đã thanh toán + còn trong hạn hoàn tiền (>= 2 ngày trước khởi hành, có/không có ví sẵn),
/// đã thanh toán + sát ngày khởi hành (< 2 ngày, không hoàn tiền).
/// </summary>
public class CancelTests
{
    public record CancelTestCase(
        string CaseName,
        bool BookingFound,
        string Status,
        double DepartingInDays,
        bool WalletExists,
        string? ExpectedException,
        string? ExpectedStatus,
        bool ExpectedRefunded,
        string? ExpectedCancelReason);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<CancelTestCase>("TestData/Booking/BookingService/Cancel.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task CancelAsync_EquivalencePartitions(CancelTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var schedule = new TourScheduleBuilder().DepartingInDays(c.DepartingInDays).Build();
        var booking = c.BookingFound
            ? new BookingBuilder().WithSchedule(schedule).WithStatus(c.Status).WithTotalPrice(500_000m).Build()
            : null;

        bookingRepo.Setup(r => r.FindUserBookingWithScheduleAsync(TestGuids.BookingId, TestGuids.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        if (c.WalletExists)
        {
            walletRepo.Setup(w => w.FindAsync(TestGuids.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserWalletBuilder().WithUserId(TestGuids.UserId).WithBalance(100_000m).Build());
        }

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions,
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object);

        var act = async () => await service.CancelAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        if (c.ExpectedException == "NotFound")
        {
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.ShouldBeNotFoundException();
            return;
        }

        if (c.ExpectedException == "AlreadyCancelled")
        {
            var exception = await act.Should().ThrowAsync<AppException>();
            exception.Which.ShouldBeAppException(9999);
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.Status.Should().Be(c.ExpectedStatus);
        result.Subject.Refunded.Should().Be(c.ExpectedRefunded);
        booking!.cancel_reason.Should().Be(c.ExpectedCancelReason);

        if (c.ExpectedRefunded)
        {
            walletRepo.Verify(w => w.Add(It.IsAny<user_wallet>()), c.WalletExists ? Times.Never() : Times.Once());
        }
        else
        {
            walletRepo.Verify(w => w.Add(It.IsAny<user_wallet>()), Times.Never);
        }

        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
