using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Blog;

namespace DDMS.Backend.Services.Interfaces;

public interface IBlogService
{
    Task<PagedResponse<BlogPostListItem>> GetPublishedAsync(
        string? category, int page, int pageSize, CancellationToken ct);

    Task<BlogPostDetail> GetBySlugAsync(string slug, CancellationToken ct);

    Task<PagedResponse<BlogPostListItem>> GetForAdminAsync(
        string? status, int page, int pageSize, CancellationToken ct);

    Task<BlogPostDetail> GetByIdAsync(Guid id, CancellationToken ct);

    Task<string> SetPublishedAsync(Guid id, bool published, CancellationToken ct);
}
