namespace DDMS.Backend.Models.DTOs.Blog;

/// <summary>Một cảnh trong video: lời đọc + ảnh minh hoạ.</summary>
public class VideoScene
{
    public string Narration { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? ImageUrl { get; set; }
    /// <summary>Thời lượng gợi ý, giây. Trình duyệt dùng để chuyển cảnh.</summary>
    public int DurationSeconds { get; set; } = 6;
}

public class BlogPostListItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SourceName { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public bool HasVideo { get; set; }
}

public class BlogPostDetail : BlogPostListItem
{
    public string? Content { get; set; }
    public DateTime? SourcePublishedAt { get; set; }
    public List<VideoScene> VideoScenes { get; set; } = new();
}

/// <summary>Kết quả một lần chạy cào tin.</summary>
public class CrawlResult
{
    public int Fetched { get; set; }
    public int Skipped { get; set; }
    public int Created { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PublishRequest
{
    public bool Published { get; set; } = true;
}
