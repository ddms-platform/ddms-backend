using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Services.Interfaces;

public interface IBlogRealtimePublisher
{
    /// <summary>Đẩy bài vừa lên trang tới mọi khách đang mở mục tin tức.</summary>
    Task PublishNewPostAsync(blog_post post, CancellationToken ct = default);
}
