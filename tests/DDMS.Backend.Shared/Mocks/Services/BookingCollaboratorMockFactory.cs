using DDMS.Backend.Models.DTOs.AdminOps;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class AdminAlertPublisherMockFactory
{
    /// <summary>Mặc định: nuốt mọi alert, không làm gì.</summary>
    public static Mock<IAdminAlertPublisher> Create()
    {
        var mock = new Mock<IAdminAlertPublisher>();
        mock.Setup(p => p.PublishAsync(It.IsAny<AlertItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}

public static class BookingPricingServiceMockFactory
{
    private const decimal StubUnitPrice = 100_000m;

    /// <summary>
    /// Mặc định phản chiếu lại đúng các dòng cabin/dịch vụ được truyền vào, gán đơn giá
    /// cố định — nhờ vậy test kiểm tra "có ghi booking_cabin không" vẫn đúng ý nghĩa.
    /// Truyền <paramref name="quote"/> để cố định kết quả khi test quan tâm tới con số cụ thể.
    /// </summary>
    public static Mock<IBookingPricingService> Create(BookingQuote? quote = null)
    {
        var mock = new Mock<IBookingPricingService>();
        mock.Setup(p => p.QuoteAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<IReadOnlyCollection<BookingLineRequest>>(),
                It.IsAny<IReadOnlyCollection<BookingLineRequest>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Guid _,
                int numPeople,
                IReadOnlyCollection<BookingLineRequest> cabins,
                IReadOnlyCollection<BookingLineRequest> services,
                string? _,
                CancellationToken _) =>
            {
                if (quote is not null) return quote;

                var cabinLines = ToPriced(cabins);
                var serviceLines = ToPriced(services);
                var stub = new BookingQuote
                {
                    BasePrice = StubUnitPrice * numPeople,
                    CabinPrice = cabinLines.Sum(l => l.UnitPrice * l.Quantity),
                    ServicePrice = serviceLines.Sum(l => l.UnitPrice * l.Quantity),
                    CabinLines = cabinLines,
                    ServiceLines = serviceLines,
                };
                stub.Subtotal = stub.BasePrice + stub.CabinPrice + stub.ServicePrice;
                stub.TotalPrice = stub.Subtotal;
                return stub;
            });
        return mock;
    }

    private static List<PricedLine> ToPriced(IReadOnlyCollection<BookingLineRequest> lines) =>
        lines.Select(l => new PricedLine
        {
            Id = l.Id,
            Quantity = l.Quantity,
            UnitPrice = StubUnitPrice,
        }).ToList();
}
