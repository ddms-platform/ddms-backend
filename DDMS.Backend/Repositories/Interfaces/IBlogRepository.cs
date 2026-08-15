using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBlogRepository
{
    Task<bool> ExistsBySourceHashAsync(string hash, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);

    Task<(List<blog_post> Items, int Total)> GetPublishedAsync(
        string? category, int page, int pageSize, CancellationToken ct);

    Task<blog_post?> FindPublishedBySlugAsync(string slug, CancellationToken ct);
    Task<blog_post?> FindByIdAsync(Guid id, CancellationToken ct);

    Task<(List<blog_post> Items, int Total)> GetAllForAdminAsync(
        string? status, int page, int pageSize, CancellationToken ct);

    void Add(blog_post entity);
    Task SaveChangesAsync(CancellationToken ct);
}
