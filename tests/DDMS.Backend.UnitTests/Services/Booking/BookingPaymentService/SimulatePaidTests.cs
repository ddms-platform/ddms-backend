using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingPaymentService;

/// <summary>
/// Giả lập thanh toán dùng để demo khi không tiện chuyển khoản thật.
///
/// Đang mở cho mọi tài khoản đã đăng nhập theo yêu cầu của kỳ bảo vệ. Điều duy
/// nhất còn phải giữ: không giả lập được đơn của người khác.
/// </summary>
public class SimulatePaidTests
{
    private const long OrderCode = 999_888L;

    private static (DDMS.Backend.Services.Implementations.BookingPaymentService Service,
                    Mock<IBookingService> BookingService)
        Build(IHostEnvironment env, booking_payment? latest)
    {
        var bookings = BookingRepositoryMockFactory.Create();
        var payments = BookingPaymentRepositoryMockFactory.Create();
        var bookingService = new Mock<IBookingService>();

        bookings.Setup(r => r.FindUserBookingAsync(TestGuids.BookingId, TestGuids.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingBuilder().WithStatus(BookingStatuses.Pending).Build());
        payments.Setup(p => p.FindLatestByBookingAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latest);

        var service = new DDMS.Backend.Services.Implementations.BookingPaymentService(
            bookings.Object,
            payments.Object,
            bookingService.Object,
            PayOSGatewayMockFactory.Create().Object,
            OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BillingOptions>(),
            env,
            NullLogger<DDMS.Backend.Services.Implementations.BookingPaymentService>.Instance);

        return (service, bookingService);
    }

    private static booking_payment PendingPayment() => new()
    {
        id = Guid.NewGuid(),
        booking_id = TestGuids.BookingId,
        amount = 500_000m,
        status = BookingPaymentStatuses.Pending,
        payos_order_code = OrderCode,
        created_at = DateTime.UtcNow,
    };

    /// <summary>
    /// Yêu cầu của kỳ bảo vệ: bấm một cái là đơn xác nhận ngay, kể cả trên
    /// production và kể cả tài khoản chỉ có vai trò "user".
    /// </summary>
    [Fact]
    public async Task TrenProduction_TaiKhoanThuong_XacNhanBookingNhuWebhookThat()
    {
        var payment = PendingPayment();
        var (service, bookingService) = Build(HostEnvironmentMockFactory.Production(), payment);

        var result = await service.SimulatePaidAsync(
            TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.Paid.Should().BeTrue();
        payment.status.Should().Be(BookingPaymentStatuses.Paid);
        payment.amount_paid.Should().Be(500_000m);
        bookingService.Verify(b => b.MarkPaidAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrenDev_XacNhanBookingNhuWebhookThat()
    {
        var payment = PendingPayment();
        var (service, bookingService) = Build(HostEnvironmentMockFactory.Development(), payment);

        var result = await service.SimulatePaidAsync(
            TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.Paid.Should().BeTrue();
        payment.status.Should().Be(BookingPaymentStatuses.Paid);
        payment.amount_paid.Should().Be(500_000m);
        bookingService.Verify(b => b.MarkPaidAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChuaTaoLinkThanhToan_Nem()
    {
        var (service, bookingService) = Build(HostEnvironmentMockFactory.Development(), latest: null);

        var act = async () =>
            await service.SimulatePaidAsync(
                TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<AppException>();
        exception.Which.ShouldBeAppException(ErrorCode.BookingPaymentNotFound);
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Ràng buộc duy nhất còn giữ sau khi mở cho mọi vai trò: không ai mượn
    /// đường này để đụng vào đơn của người khác. FindUserBookingAsync vẫn lọc
    /// theo chủ đơn.
    /// </summary>
    [Fact]
    public async Task DonCuaNguoiKhac_Nem_KhongXacNhanGi()
    {
        var payment = PendingPayment();
        var (service, bookingService) = Build(HostEnvironmentMockFactory.Production(), payment);
        var nguoiKhac = Guid.NewGuid();

        var act = async () =>
            await service.SimulatePaidAsync(
                TestGuids.BookingId, nguoiKhac, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        payment.status.Should().Be(BookingPaymentStatuses.Pending);
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
