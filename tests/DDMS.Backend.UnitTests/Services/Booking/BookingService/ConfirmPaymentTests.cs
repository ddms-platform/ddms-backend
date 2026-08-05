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
/// Equivalence Partitioning cho BookingService.ConfirmPaymentAsync — xem TestData/Booking/BookingService/ConfirmPayment.json.
/// Lớp tương đương: không tìm thấy booking, trạng thái không phải pending/holding (no-op), holding còn hạn/hết hạn/không có hạn,
/// và nhánh gửi thông báo cho chủ tàu (owner_id có/không).
/// </summary>
public class ConfirmPaymentTests
{
    public record ConfirmPaymentTestCase(
        string CaseName,
        bool BookingFound,
        string Status,
        int? HoldExpiredAtOffsetHours,
        bool BoatExists,
        bool OwnerIdPresent,
        bool FullNamePresent,
        string? ExpectedException,
        bool ExpectNoOp,
        bool ExpectOwnerNotification);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<ConfirmPaymentTestCase>("TestData/Booking/BookingService/ConfirmPayment.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ConfirmPaymentAsync_EquivalencePartitions(ConfirmPaymentTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

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
            ? new BookingBuilder().WithSchedule(schedule).WithUser(user).WithStatus(c.Status).WithHoldExpiredAt(holdExpiredAt).Build()
            : null;

        bookingRepo.Setup(r => r.FindUserBookingWithDetailsAsync(TestGuids.BookingId, TestGuids.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions);

        var act = async () => await service.ConfirmPaymentAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        if (c.ExpectedException == "NotFound")
        {
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.ShouldBeNotFoundException();
            return;
        }

        if (c.ExpectedException == "HoldExpired")
        {
            var exception = await act.Should().ThrowAsync<AppException>();
            exception.Which.ShouldBeAppException(ErrorCode.HoldExpired);
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            return;
        }

        await act.Should().NotThrowAsync();

        if (c.ExpectNoOp)
        {
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
