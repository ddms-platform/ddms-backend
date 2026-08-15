using DDMS.Backend.Models.DTOs.Blog;

namespace DDMS.Backend.Services.Interfaces;

public interface IBlogCrawlerService
{
    /// <summary>
    /// Lấy tin du lịch Đà Nẵng từ RSS, nhờ Gemini viết lại, lưu thành bài nháp.
    /// Bài mới luôn ở trạng thái draft — phải có người duyệt mới lên trang.
    /// </summary>
    Task<CrawlResult> CrawlAsync(CancellationToken ct);
}
