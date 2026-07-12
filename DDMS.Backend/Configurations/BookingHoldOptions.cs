using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Configurations;

/// <summary>
/// Cấu hình thời gian giữ chỗ (Seat Hold). Thời gian giữ của Đại lý (B2B)
/// thay đổi động theo số ngày còn lại tới ngày khởi hành (Dynamic Hold Time).
/// </summary>
public class BookingHoldOptions
{
    public const string SectionName = "BookingHold";

    /// <summary>
    /// Nếu tour khởi hành trong vòng số ngày này thì CẤM giữ chỗ với mọi đối tượng
    /// (phải thanh toán ngay). Ví dụ 1 = tour khởi hành trong vòng 1 ngày (ngày mai).
    /// </summary>
    [Range(0, 30)]
    public double NoHoldWithinDays { get; set; } = 1;

    /// <summary>Thời gian giữ chỗ cho Khách lẻ (B2C), tính bằng phút.</summary>
    [Range(1, 1440)]
    public int B2CHoldMinutes { get; set; } = 30;

    // --- Đại lý (B2B): giữ chỗ động theo ngày khởi hành ---

    /// <summary>Ngưỡng "tour còn xa": khởi hành sau >= số ngày này → giữ dài nhất.</summary>
    [Range(1, 365)]
    public double B2BLongTermDays { get; set; } = 30;

    /// <summary>Số giờ giữ chỗ khi tour còn xa (>= B2BLongTermDays).</summary>
    [Range(1, 720)]
    public int B2BLongTermHours { get; set; } = 48;

    /// <summary>Ngưỡng "tour tầm trung": khởi hành sau >= số ngày này → giữ vừa.</summary>
    [Range(1, 365)]
    public double B2BMidTermDays { get; set; } = 7;

    /// <summary>Số giờ giữ chỗ khi tour tầm trung (>= B2BMidTermDays).</summary>
    [Range(1, 720)]
    public int B2BMidTermHours { get; set; } = 24;

    /// <summary>Số giờ giữ chỗ khi tour sắp tới (dưới B2BMidTermDays nhưng vẫn cho giữ).</summary>
    [Range(1, 720)]
    public int B2BShortTermHours { get; set; } = 4;
}
