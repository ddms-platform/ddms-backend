using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Configurations;

public class BlogCrawlerOptions
{
    public const string SectionName = "BlogCrawler";

    /// <summary>
    /// Các truy vấn Google News RSS. Dùng RSS thay vì cào HTML: đây là kênh
    /// các báo chủ động phát hành để tổng hợp, ổn định hơn và không phải
    /// lách chặn bot.
    /// </summary>
    [Required, MinLength(1)]
    public string[] Queries { get; set; } = Array.Empty<string>();

    /// <summary>Số bài lấy tối đa mỗi lần chạy, tránh đốt quota Gemini.</summary>
    [Range(1, 50)]
    public int MaxItemsPerRun { get; set; } = 5;

    /// <summary>Bỏ qua bài cũ hơn số ngày này.</summary>
    [Range(1, 365)]
    public int MaxAgeDays { get; set; } = 30;
}
