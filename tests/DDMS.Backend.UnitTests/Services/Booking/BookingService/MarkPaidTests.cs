using DDMS.Backend.Common.Constants;
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
/// Equivalence Partitioning cho BookingService.MarkPaidAsync — xem TestData/Booking/BookingService/MarkPaid.json.
///
/// Hàm này chỉ được gọi sau khi PayOS đã xác nhận có tiền, nên nó không tự kiểm tra tiền nữa.
/// Lớp tương đương: không tìm thấy booking, trạng thái đã xử lý rồi (no-op),
/// pending/holding (xác nhận + thông báo), holding hết hạn (vẫn xác nhận vì tiền đã vào),
/// và đơn đã huỷ mà tiền vẫn vào (phải hoàn lại ví khách thay vì nuốt).
/// </summary>
public class MarkPaidTests
{
    public record MarkPaidTestCase(
        string CaseName,
        bool BookingFound,
        string Status,
        int? HoldExpiredAtOffsetHours,
        decimal PaidAmount,
        bool BoatExists,
        bool OwnerIdPresent,
        bool FullNamePresent,
        string? ExpectedException,
        bool ExpectNoOp,
        bool ExpectRefund,
        bool ExpectOwnerNotification);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<MarkPaidTestCase>("TestData/Booking/BookingService/MarkPaid.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task MarkPaidAsync_EquivalencePartitions(MarkPaidTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();
        var paymentRepo = c.PaidAmount > 0
            ? BookingPaymentRepositoryMockFactory.CreatePaid(c.PaidAmount)
            : BookingPaymentRepositoryMockFactory.Create();

        DateTime? holdExpiredAt = c.HoldExpiredAtOffsetHours.HasValue
            ? DateTime.UtcNow.AddHours(c.HoldExpiredAtOffsetHours.Value)
            : null;

        var boat = c.BoatExists
            ? new BoatBuilder().WithOwnerId(c.OwnerIdPresent ? TestGuids.OwnerId : null).Build()
            : null;
        var schedule = new TourScheduleBuilder().WithBoat(boat).DepartingInDays(10).Build();
        var user = new UserBuilder().WithId(TestGuids.UserId).Build();
        if (!c.FullNamePresent) user.full_name = null!;
        var booking = c.BookingFound
            ? new BookingBuilder()
                .WithSchedule(schedule)
                .WithUser(user)
                .WithStatus(c.Status)
                .WithHoldExpiredAt(holdExpiredAt)
                .Build()
            : null;

        bookingRepo.Setup(r => r.FindBookingWithDetailsAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions,
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object,
            paymentRepo.Object);

        var act = async () => await service.MarkPaidAsync(TestGuids.BookingId, CancellationToken.None);

        if (c.ExpectedException == "NotFound")
        {
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.ShouldBeNotFoundException();
            return;
        }

        await act.Should().NotThrowAsync();

        if (c.ExpectRefund)
        {
            // Tiền vào sau khi đơn đã huỷ: phải cộng lại vào ví khách, không giữ im.
            walletRepo.Verify(w => w.Add(It.Is<user_wallet>(x => x.user_id == TestGuids.UserId)), Times.Once);
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            booking!.status.Should().Be(BookingStatuses.Cancelled);
            return;
        }

        if (c.ExpectNoOp)
        {
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            walletRepo.Verify(w => w.Add(It.IsAny<user_wallet>()), Times.Never);
            booking!.status.Should().Be(c.Status);
            return;
        }

        booking!.status.Should().Be(BookingStatuses.Confirmed);
        booking.hold_expired_at.Should().BeNull();
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        notificationService.Verify(n => n.CreateNotificationAsync(
            null, "system", It.IsAny<string>(), It.IsAny<string>(),
            It.Is<List<Guid>>(l => l.Contains(TestGuids.UserId)), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);

        notificationService.Verify(n => n.CreateNotificationAsync(
            null, "owner", It.IsAny<string>(), It.IsAny<string>(),
            It.Is<List<Guid>>(l => l.Contains(TestGuids.OwnerId)), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            c.ExpectOwnerNotification ? Times.Once() : Times.Never());
    }
}
