using DDMS.Backend.Configurations;

namespace DDMS.Backend.Common.Helpers;

/// <summary>
/// Tính thời gian giữ chỗ (Dynamic Hold Time) dựa trên loại khách và ngày khởi hành.
///
/// Luật nghiệp vụ:
///  - Tour khởi hành quá sát (trong vòng NoHoldWithinDays) → CẤM giữ chỗ, phải trả ngay.
///  - Khách lẻ (B2C): giữ ngắn (B2CHoldMinutes phút) chỉ để nhập thẻ.
///  - Đại lý (B2B): giữ dài, giảm dần khi càng sát ngày khởi hành:
///      xa (>= 30 ngày) → 48h;  tầm trung (>= 7 ngày) → 24h;  sắp tới → 4h.
/// </summary>
public static class HoldPolicy
{
    /// <summary>
    /// Trả về khoảng thời gian được giữ chỗ.
    /// Trả về <c>null</c> nếu KHÔNG được phép giữ chỗ (phải thanh toán ngay).
    /// </summary>
    public static TimeSpan? CalculateHoldDuration(
        bool isAgent,
        DateTime departureUtc,
        DateTime nowUtc,
        BookingHoldOptions opt)
    {
        var daysToDeparture = (departureUtc - nowUtc).TotalDays;

        // Luật 3: sát giờ khởi hành → cấm giữ chỗ với MỌI đối tượng.
        if (daysToDeparture < opt.NoHoldWithinDays)
            return null;

        // Khách lẻ: giữ ngắn cố định.
        if (!isAgent)
            return TimeSpan.FromMinutes(opt.B2CHoldMinutes);

        // Đại lý: động theo ngày khởi hành.
        if (daysToDeparture >= opt.B2BLongTermDays)
            return TimeSpan.FromHours(opt.B2BLongTermHours);   // tour xa → 48h
        if (daysToDeparture >= opt.B2BMidTermDays)
            return TimeSpan.FromHours(opt.B2BMidTermHours);    // tầm trung → 24h
        return TimeSpan.FromHours(opt.B2BShortTermHours);      // sắp tới → 4h
    }

    /// <summary>Có được phép giữ chỗ không (tiện dùng nhanh).</summary>
    public static bool CanHold(bool isAgent, DateTime departureUtc, DateTime nowUtc, BookingHoldOptions opt)
        => CalculateHoldDuration(isAgent, departureUtc, nowUtc, opt) != null;
}
