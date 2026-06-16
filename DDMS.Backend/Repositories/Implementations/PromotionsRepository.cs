using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class PromotionsRepository : IPromotionsRepository
{
    private readonly AppDbContext _db;
    public PromotionsRepository(AppDbContext db) => _db = db;

    public Task<List<promotion>> GetAllWithCreatorAsync(CancellationToken ct) =>
        _db.promotions
            .Include(p => p.created_byNavigation)
                .ThenInclude(u => u!.user_roles)
                    .ThenInclude(ur => ur.role)
            .OrderByDescending(p => p.created_at)
            .ToListAsync(ct);

    public Task<List<promotion>> GetByOwnerAsync(Guid ownerId, CancellationToken ct) =>
        _db.promotions
            .Where(p => p.created_by == ownerId)
            .OrderByDescending(p => p.created_at)
            .ToListAsync(ct);

    public Task<promotion?> FindAsync(Guid id, CancellationToken ct) =>
        _db.promotions.FindAsync(new object?[] { id }, ct).AsTask();

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        excludeId.HasValue
            ? _db.promotions.AnyAsync(p => p.code == code && p.id != excludeId.Value, ct)
            : _db.promotions.AnyAsync(p => p.code == code, ct);

    public void Add(promotion entity) => _db.promotions.Add(entity);
    public void Remove(promotion entity) => _db.promotions.Remove(entity);
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
