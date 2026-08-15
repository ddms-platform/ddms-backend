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

    /// <summary>Bật worker chạy nền. Tắt thì chỉ cào khi admin bấm tay.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Chu kỳ quét, phút. RSS không có cơ chế đẩy nên "tức thì" ở đây nghĩa là
    /// hỏi lại nguồn theo chu kỳ ngắn, chứ không phải nhận thông báo từ báo.
    /// </summary>
    [Range(1, 1440)]
    public int IntervalMinutes { get; set; } = 15;

    /// <summary>
    /// Cho bài lên trang ngay, không chờ người duyệt. Bài vẫn phải qua bộ lọc
    /// chất lượng tự động trong BlogCrawlerService.
    /// </summary>
    public bool AutoPublish { get; set; } = true;

    /// <summary>Số ký tự tối thiểu của nội dung thì mới cho tự đăng.</summary>
    [Range(0, 5000)]
    public int MinContentLength { get; set; } = 320;

    /// <summary>Số cảnh video tối thiểu thì mới cho tự đăng.</summary>
    [Range(0, 20)]
    public int MinVideoScenes { get; set; } = 3;
}
