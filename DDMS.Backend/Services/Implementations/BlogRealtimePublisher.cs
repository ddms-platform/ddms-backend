using DDMS.Backend.Hubs;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Services.Implementations;

public class BlogRealtimePublisher : IBlogRealtimePublisher
{
    private readonly IHubContext<BlogHub> _hub;
    private readonly ILogger<BlogRealtimePublisher> _logger;

    public BlogRealtimePublisher(IHubContext<BlogHub> hub, ILogger<BlogRealtimePublisher> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task PublishNewPostAsync(blog_post post, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                id = post.id,
                title = post.title,
                slug = post.slug,
                summary = post.summary,
                coverImageUrl = post.cover_image_url,
                category = post.category,
                sourceName = post.source_name,
                sourceUrl = post.source_url,
                publishedAt = post.published_at,
                viewCount = post.view_count,
                hasVideo = !string.IsNullOrWhiteSpace(post.video_script),
            };

            await _hub.Clients.Group(BlogHub.GroupName).SendAsync("NewPost", payload, ct);
        }
        catch (Exception ex)
        {
            // Đẩy realtime hỏng thì bài vẫn đã lưu và vẫn hiện khi tải lại trang.
            _logger.LogWarning(ex, "Không đẩy được bài {Slug} qua SignalR", post.slug);
        }
    }
}
