using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingPaymentService;

/// <summary>
/// Webhook PayOS là một trong hai đường duy nhất làm booking chuyển sang đã thanh toán.
/// Các test ở đây giữ đúng một tính chất: không có chữ ký hợp lệ thì không có gì được xác nhận.
/// </summary>
public class HandleWebhookTests
{
    private const long OrderCode = 1_234_567L;

    private static (DDMS.Backend.Services.Implementations.BookingPaymentService Service,
                    Mock<IBookingService> BookingService,
                    Mock<IBookingPaymentRepository> Payments)
        Build(Mock<IPayOSGateway> gateway, booking_payment? existing)
    {
        var bookings = BookingRepositoryMockFactory.Create();
        var payments = BookingPaymentRepositoryMockFactory.Create();
        var bookingService = new Mock<IBookingService>();

        payments.Setup(p => p.FindByOrderCodeAsync(OrderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = new DDMS.Backend.Services.Implementations.BookingPaymentService(
            bookings.Object,
            payments.Object,
            bookingService.Object,
            gateway.Object,
            OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BillingOptions>(),
            NullLogger<DDMS.Backend.Services.Implementations.BookingPaymentService>.Instance);

        return (service, bookingService, payments);
    }

    private static booking_payment PendingPayment() => new()
    {
        id = Guid.NewGuid(),
        booking_id = TestGuids.BookingId,
        amount = 500_000m,
        amount_paid = 0m,
        status = BookingPaymentStatuses.Pending,
        payos_order_code = OrderCode,
        created_at = DateTime.UtcNow,
    };

    [Fact]
    public async Task ChuKySai_KhongXacNhanBooking()
    {
        // Gateway mặc định trả null = chữ ký không hợp lệ.
        var payment = PendingPayment();
        var (service, bookingService, payments) = Build(PayOSGatewayMockFactory.Create(), payment);

        var result = await service.HandleWebhookAsync(new Webhook(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("01");
        payment.status.Should().Be(BookingPaymentStatuses.Pending);
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        payments.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChuKyDung_NhungKhongCoDonTuongUng_KhongXacNhanGi()
    {
        var gateway = PayOSGatewayMockFactory.WithValidWebhook(OrderCode, 500_000);
        var (service, bookingService, _) = Build(gateway, existing: null);

        var result = await service.HandleWebhookAsync(new Webhook(), CancellationToken.None);

        // Trả 00 để PayOS thôi gửi lại, nhưng tuyệt đối không xác nhận booking nào.
        result.IsSuccess.Should().BeTrue();
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChuKyDung_DonKhop_GhiNhanTienVaXacNhanBooking()
    {
        var payment = PendingPayment();
        var gateway = PayOSGatewayMockFactory.WithValidWebhook(OrderCode, 500_000);
        var (service, bookingService, _) = Build(gateway, payment);

        var result = await service.HandleWebhookAsync(new Webhook(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.status.Should().Be(BookingPaymentStatuses.Paid);
        payment.amount_paid.Should().Be(500_000m);
        payment.paid_at.Should().NotBeNull();
        bookingService.Verify(b => b.MarkPaidAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoiLaiLanHai_KhongXacNhanHaiLan()
    {
        var payment = PendingPayment();
        var gateway = PayOSGatewayMockFactory.WithValidWebhook(OrderCode, 500_000);
        var (service, bookingService, _) = Build(gateway, payment);

        await service.HandleWebhookAsync(new Webhook(), CancellationToken.None);
        await service.HandleWebhookAsync(new Webhook(), CancellationToken.None);

        // PayOS gửi lại webhook là chuyện bình thường; trừ lượt mã giảm giá hai lần thì không.
        bookingService.Verify(b => b.MarkPaidAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
