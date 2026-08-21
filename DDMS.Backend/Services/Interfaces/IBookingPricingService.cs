using DDMS.Backend.Models.DTOs.Booking;

namespace DDMS.Backend.Services.Interfaces;

public interface IBookingPricingService
{
    /// <summary>
    /// Tính giá một booking hoàn toàn từ dữ liệu server: giá tour theo lịch trình,
    /// giá cabin và dịch vụ tra theo id. Nếu có <paramref name="code"/> thì kiểm tra
    /// mã giảm giá và áp vào; mã không hợp lệ sẽ ném <see cref="Common.Exceptions.AppException"/>
    /// với mã lỗi cụ thể cho từng nguyên nhân.
    /// </summary>
    Task<BookingQuote> QuoteAsync(
        Guid scheduleId,
        PartyComposition party,
        IReadOnlyCollection<BookingLineRequest> cabins,
        IReadOnlyCollection<BookingLineRequest> services,
        string? code,
        CancellationToken ct);
}
