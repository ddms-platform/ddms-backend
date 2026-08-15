using System.Text.Json;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Blog;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class BlogService : IBlogService
{
    private readonly IBlogRepository _repo;

    public BlogService(IBlogRepository repo) => _repo = repo;

    public async Task<PagedResponse<BlogPostListItem>> GetPublishedAsync(
        string? category, int page, int pageSize, CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 9 : pageSize;

        var (items, total) = await _repo.GetPublishedAsync(category, page, pageSize, ct);
        return new PagedResponse<BlogPostListItem>
        {
            items = items.Select(MapListItem).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<BlogPostDetail> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var post = await _repo.FindPublishedBySlugAsync(slug, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy bài viết.");

        post.view_count += 1;
        await _repo.SaveChangesAsync(ct);

        return MapDetail(post);
    }

    public async Task<PagedResponse<BlogPostListItem>> GetForAdminAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, total) = await _repo.GetAllForAdminAsync(status, page, pageSize, ct);
        return new PagedResponse<BlogPostListItem>
        {
            items = items.Select(MapListItem).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<BlogPostDetail> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var post = await _repo.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy bài viết.");
        return MapDetail(post);
    }

    public async Task<string> SetPublishedAsync(Guid id, bool published, CancellationToken ct)
    {
        var post = await _repo.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy bài viết.");

        // Không cho xuất bản bài mất dẫn nguồn: nội dung là tác phẩm phái sinh
        // từ bài gốc, bỏ nguồn đi là đăng lại của người khác dưới tên mình.
        if (published && string.IsNullOrWhiteSpace(post.source_url))
            throw new AppException(ErrorCode.UncategorizedError,
                "Bài viết chưa có liên kết nguồn nên không thể xuất bản.");

        post.status = published ? "published" : "draft";
        post.published_at = published ? (post.published_at ?? DateTime.UtcNow) : null;
        post.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);

        return published ? "Đã xuất bản bài viết." : "Đã gỡ bài viết khỏi trang.";
    }

    private static BlogPostListItem MapListItem(blog_post p) => new()
    {
        Id = p.id,
        Title = p.title,
        Slug = p.slug,
        Summary = p.summary,
        CoverImageUrl = p.cover_image_url,
        Category = p.category,
        SourceName = p.source_name,
        SourceUrl = p.source_url,
        PublishedAt = p.published_at,
        ViewCount = p.view_count,
        HasVideo = !string.IsNullOrWhiteSpace(p.video_script),
    };

    private static BlogPostDetail MapDetail(blog_post p)
    {
        var scenes = new List<VideoScene>();
        if (!string.IsNullOrWhiteSpace(p.video_script))
        {
            try { scenes = JsonSerializer.Deserialize<List<VideoScene>>(p.video_script!) ?? new(); }
            catch { /* kịch bản hỏng thì coi như không có video, không làm gãy trang */ }
        }

        return new BlogPostDetail
        {
            Id = p.id,
            Title = p.title,
            Slug = p.slug,
            Summary = p.summary,
            Content = p.content,
            CoverImageUrl = p.cover_image_url,
            Category = p.category,
            SourceName = p.source_name,
            SourceUrl = p.source_url,
            SourcePublishedAt = p.source_published_at,
            PublishedAt = p.published_at,
            ViewCount = p.view_count,
            HasVideo = scenes.Count > 0,
            VideoScenes = scenes,
        };
    }
}
