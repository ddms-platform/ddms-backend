using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BlogRepository : IBlogRepository
{
    private const string Published = "published";

    private readonly AppDbContext _db;

    public BlogRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsBySourceHashAsync(string hash, CancellationToken ct) =>
        _db.blog_posts.AnyAsync(p => p.source_hash == hash, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        _db.blog_posts.AnyAsync(p => p.slug == slug, ct);

    public async Task<(List<blog_post>, int)> GetPublishedAsync(
        string? category, int page, int pageSize, CancellationToken ct)
    {
        var q = _db.blog_posts.Where(p => p.status == Published);
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => p.category == category);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.published_at ?? p.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<blog_post?> FindPublishedBySlugAsync(string slug, CancellationToken ct) =>
        _db.blog_posts.FirstOrDefaultAsync(p => p.slug == slug && p.status == Published, ct);

    public Task<blog_post?> FindByIdAsync(Guid id, CancellationToken ct) =>
        _db.blog_posts.FirstOrDefaultAsync(p => p.id == id, ct);

    public async Task<(List<blog_post>, int)> GetAllForAdminAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var q = _db.blog_posts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.status == status);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Add(blog_post entity) => _db.blog_posts.Add(entity);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
