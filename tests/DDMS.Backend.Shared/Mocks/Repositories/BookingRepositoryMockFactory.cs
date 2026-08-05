using DDMS.Backend.Repositories.Interfaces;
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
        return mock;
    }
}
