using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PayOS.Models.V2.PaymentRequests;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingPaymentService;

/// <summary>
/// Đường thứ hai để booking được xác nhận: frontend hỏi trạng thái, server tự đối chiếu với PayOS.
/// Điểm mấu chốt là client không nói được gì ngoài id đơn — trạng thái luôn lấy từ PayOS.
/// </summary>
public class SyncStatusTests
{
    private const long OrderCode = 7_654_321L;

    private static (DDMS.Backend.Services.Implementations.BookingPaymentService Service,
                    Mock<IBookingService> BookingService)
        Build(Mock<IPayOSGateway> gateway, booking_payment? latest, string bookingStatus = BookingStatuses.Pending)
    {
        var bookings = BookingRepositoryMockFactory.Create();
        var payments = BookingPaymentRepositoryMockFactory.Create();
        var bookingService = new Mock<IBookingService>();

        var booking = new BookingBuilder().WithStatus(bookingStatus).Build();
        bookings.Setup(r => r.FindUserBookingAsync(TestGuids.BookingId, TestGuids.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        payments.Setup(p => p.FindLatestByBookingAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latest);

        var service = new DDMS.Backend.Services.Implementations.BookingPaymentService(
            bookings.Object,
            payments.Object,
            bookingService.Object,
            gateway.Object,
            OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BillingOptions>(),
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

    [Fact]
    public async Task ChuaTaoLinkThanhToan_TraVeNone()
    {
        var (service, bookingService) = Build(PayOSGatewayMockFactory.Create(), latest: null);

        var result = await service.SyncStatusAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.PaymentStatus.Should().Be("none");
        result.Paid.Should().BeFalse();
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PayOSBaoChuaTra_KhongXacNhanBooking()
    {
        var payment = PendingPayment();
        var gateway = PayOSGatewayMockFactory.WithLinkStatus(OrderCode, PaymentLinkStatus.Pending);
        var (service, bookingService) = Build(gateway, payment);

        var result = await service.SyncStatusAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.Paid.Should().BeFalse();
        payment.status.Should().Be(BookingPaymentStatuses.Pending);
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PayOSBaoDaTra_XacNhanBooking()
    {
        var payment = PendingPayment();
        var gateway = PayOSGatewayMockFactory.WithLinkStatus(OrderCode, PaymentLinkStatus.Paid, 500_000);
        var (service, bookingService) = Build(gateway, payment);

        var result = await service.SyncStatusAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.Paid.Should().BeTrue();
        payment.status.Should().Be(BookingPaymentStatuses.Paid);
        payment.amount_paid.Should().Be(500_000m);
        bookingService.Verify(b => b.MarkPaidAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PayOSBaoHetHan_DanhDauExpired_KhongXacNhan()
    {
        var payment = PendingPayment();
        var gateway = PayOSGatewayMockFactory.WithLinkStatus(OrderCode, PaymentLinkStatus.Expired);
        var (service, bookingService) = Build(gateway, payment);

        await service.SyncStatusAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        payment.status.Should().Be(BookingPaymentStatuses.Expired);
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PayOSKhongTraLoi_GiuNguyenPending_KhongXacNhan()
    {
        // Gateway mặc định trả null cho GetPaymentLinkAsync = không hỏi được PayOS.
        var payment = PendingPayment();
        var (service, bookingService) = Build(PayOSGatewayMockFactory.Create(), payment);

        var result = await service.SyncStatusAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        result.Paid.Should().BeFalse();
        payment.status.Should().Be(BookingPaymentStatuses.Pending);
        bookingService.Verify(b => b.MarkPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
