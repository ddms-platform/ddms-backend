using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

/// <summary>
/// Tạo Mock&lt;IBookingRepository&gt; với default an toàn (SaveChangesAsync no-op, Add* no-op).
/// Mỗi test override thêm đúng method cần cho equivalence class của nó.
/// </summary>
public static class BookingRepositoryMockFactory
{
    public static Mock<IBookingRepository> Create()
    {
        var mock = new Mock<IBookingRepository>();
        mock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.AddBooking(It.IsAny<Models.Entities.booking>()));
        mock.Setup(r => r.AddBookingCabin(It.IsAny<Models.Entities.booking_cabin>()));
        mock.Setup(r => r.AddBookingService(It.IsAny<Models.Entities.booking_service>()));

        // Mọi test đặt chỗ đều đi qua giao dịch, nên default phải có sẵn
        // — test nào quan tâm tới commit/rollback thì tự override.
        var tx = new Mock<IDbContextTransaction>();
        tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        tx.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        mock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx.Object);
        mock.Setup(r => r.LockScheduleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
