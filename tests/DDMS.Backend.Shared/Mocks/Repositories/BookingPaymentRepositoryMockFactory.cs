using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

public static class BookingPaymentRepositoryMockFactory
{
    /// <summary>
    /// Mặc định: booking chưa có bản ghi thanh toán nào — giống các đơn tạo trước khi
    /// bảng booking_payment tồn tại. Test nào cần mô phỏng đã trả tiền thì override lại.
    /// </summary>
    public static Mock<IBookingPaymentRepository> Create()
    {
        var mock = new Mock<IBookingPaymentRepository>();

        mock.Setup(r => r.FindPendingByBookingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((booking_payment?)null);
        mock.Setup(r => r.FindByOrderCodeAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((booking_payment?)null);
        mock.Setup(r => r.FindLatestByBookingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((booking_payment?)null);
        mock.Setup(r => r.GetPaidAmountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        mock.Setup(r => r.HasAnyPaymentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>Booking đã được PayOS xác nhận trả <paramref name="paidAmount"/>.</summary>
    public static Mock<IBookingPaymentRepository> CreatePaid(decimal paidAmount)
    {
        var mock = Create();
        mock.Setup(r => r.HasAnyPaymentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Setup(r => r.GetPaidAmountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paidAmount);
        return mock;
    }
}
