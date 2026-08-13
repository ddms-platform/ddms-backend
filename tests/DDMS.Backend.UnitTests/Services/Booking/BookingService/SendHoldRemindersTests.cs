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
/// Equivalence Partitioning cho BookingService.SendHoldRemindersAsync:
/// không có hold nào cần nhắc, gửi email thành công, gửi email lỗi (bị nuốt exception, không đánh dấu đã gửi),
/// và trộn cả 2 trong cùng 1 lượt (chỉ SaveChanges khi có ít nhất 1 email gửi thành công).
/// </summary>
public class SendHoldRemindersTests
{
    private static (
        Mock<DDMS.Backend.Repositories.Interfaces.IBookingRepository> bookingRepo,
        Mock<DDMS.Backend.Repositories.Interfaces.IWalletRepository> walletRepo,
        Mock<DDMS.Backend.Services.Interfaces.IEmailSender> emailSender,
        Mock<DDMS.Backend.Services.Interfaces.INotificationService> notificationService,
        DDMS.Backend.Services.Implementations.BookingService service) CreateSut()
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions,
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object);

        return (bookingRepo, walletRepo, emailSender, notificationService, service);
    }

    [Fact]
    public async Task SendHoldRemindersAsync_NoHolds_ReturnsZero_DoesNotSave()
    {
        var (bookingRepo, _, _, _, service) = CreateSut();
        bookingRepo.Setup(r => r.GetHoldsNeedingReminderAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), RoleNames.Agent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<booking>());

        var result = await service.SendHoldRemindersAsync(CancellationToken.None);

        result.Should().Be(0);
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendHoldRemindersAsync_EmailSucceeds_WithFullName_MarksSentAndSaves()
    {
        var (bookingRepo, _, emailSender, _, service) = CreateSut();
        var user = new UserBuilder().WithFullName("Nguyen Van A").Build();
        var booking = new BookingBuilder().WithUser(user).WithHoldExpiredAt(DateTime.UtcNow.AddHours(1)).Build();
        bookingRepo.Setup(r => r.GetHoldsNeedingReminderAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), RoleNames.Agent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<booking> { booking });

        var result = await service.SendHoldRemindersAsync(CancellationToken.None);

        result.Should().Be(1);
        booking.hold_reminder_sent.Should().BeTrue();
        emailSender.Verify(e => e.SendHoldReminderEmailAsync(
            user.email, "Nguyen Van A", It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendHoldRemindersAsync_EmailSucceeds_NoFullName_UsesDefaultGreeting()
    {
        var (bookingRepo, _, emailSender, _, service) = CreateSut();
        var user = new UserBuilder().Build();
        user.full_name = null!;
        var booking = new BookingBuilder().WithUser(user).WithHoldExpiredAt(DateTime.UtcNow.AddHours(1)).Build();
        bookingRepo.Setup(r => r.GetHoldsNeedingReminderAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), RoleNames.Agent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<booking> { booking });

        var result = await service.SendHoldRemindersAsync(CancellationToken.None);

        result.Should().Be(1);
        booking.hold_reminder_sent.Should().BeTrue();
        emailSender.Verify(e => e.SendHoldReminderEmailAsync(
            user.email, "Quý khách", It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendHoldRemindersAsync_EmailThrows_SwallowsError_DoesNotMarkSent_DoesNotSave()
    {
        var (bookingRepo, _, emailSender, _, service) = CreateSut();
        var booking = new BookingBuilder().WithHoldExpiredAt(DateTime.UtcNow.AddHours(1)).Build();
        bookingRepo.Setup(r => r.GetHoldsNeedingReminderAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), RoleNames.Agent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<booking> { booking });
        emailSender.Setup(e => e.SendHoldReminderEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("SMTP down"));

        var result = await service.SendHoldRemindersAsync(CancellationToken.None);

        result.Should().Be(0);
        booking.hold_reminder_sent.Should().NotBe(true);
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendHoldRemindersAsync_MixedResults_SavesOnceForPartialSuccess()
    {
        var (bookingRepo, _, emailSender, _, service) = CreateSut();
        var goodUser = new UserBuilder().WithId(TestGuids.UserId).WithEmail("good@example.com").Build();
        var badUser = new UserBuilder().WithId(TestGuids.OtherUserId).WithEmail("bad@example.com").Build();
        var goodBooking = new BookingBuilder().WithId(TestGuids.BookingId).WithUser(goodUser).WithHoldExpiredAt(DateTime.UtcNow.AddHours(1)).Build();
        var badBooking = new BookingBuilder().WithId(Guid.NewGuid()).WithUser(badUser).WithHoldExpiredAt(DateTime.UtcNow.AddHours(1)).Build();

        bookingRepo.Setup(r => r.GetHoldsNeedingReminderAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), RoleNames.Agent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<booking> { goodBooking, badBooking });

        emailSender.Setup(e => e.SendHoldReminderEmailAsync(
                goodBooking.user.email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);
        emailSender.Setup(e => e.SendHoldReminderEmailAsync(
                badBooking.user.email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("SMTP down"));

        var result = await service.SendHoldRemindersAsync(CancellationToken.None);

        result.Should().Be(1);
        goodBooking.hold_reminder_sent.Should().BeTrue();
        badBooking.hold_reminder_sent.Should().NotBe(true);
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
