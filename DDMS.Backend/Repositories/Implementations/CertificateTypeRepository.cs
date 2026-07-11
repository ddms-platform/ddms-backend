using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class CertificateTypeRepository : ICertificateTypeRepository
{
    private readonly AppDbContext _db;

    public CertificateTypeRepository(AppDbContext db) => _db = db;

    public Task<List<certificate_type>> GetAllAsync(string? scope = null, CancellationToken ct = default)
    {
        var query = _db.certificate_types.AsQueryable();
        if (!string.IsNullOrWhiteSpace(scope))
            query = query.Where(t => t.scope == scope);

        return query
            .OrderBy(t => t.sort_order)
            .ThenBy(t => t.id)
            .ToListAsync(ct);
    }

    public Task<List<certificate_type>> GetActiveAsync(string? scope = null, CancellationToken ct = default)
    {
        var query = _db.certificate_types.Where(t => t.is_active);
        if (!string.IsNullOrWhiteSpace(scope))
            query = query.Where(t => t.scope == scope);

        return query
            .OrderBy(t => t.sort_order)
            .ThenBy(t => t.id)
            .ToListAsync(ct);
    }

    public Task<certificate_type?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.certificate_types.FirstOrDefaultAsync(t => t.id == id, ct);

    public Task<certificate_type?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        _db.certificate_types.FirstOrDefaultAsync(t => t.code == code, ct);

    public Task<bool> ExistsActiveCodeAsync(string code, string scope, CancellationToken ct = default) =>
        _db.certificate_types.AnyAsync(t => t.code == code && t.scope == scope && t.is_active, ct);

    public async Task<bool> IsCodeInUseAsync(string code, CancellationToken ct = default)
    {
        if (await _db.boat_certificates.AnyAsync(c => c.certificate_type == code, ct))
            return true;
        return await _db.owner_documents.AnyAsync(d => d.document_type == code, ct);
    }

    public async Task<certificate_type> AddAsync(certificate_type entity, CancellationToken ct = default)
    {
        _db.certificate_types.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(certificate_type entity, CancellationToken ct = default)
    {
        _db.certificate_types.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(certificate_type entity, CancellationToken ct = default)
    {
        _db.certificate_types.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxSortOrderAsync(CancellationToken ct = default)
    {
        if (!await _db.certificate_types.AnyAsync(ct)) return 0;
        return await _db.certificate_types.MaxAsync(t => t.sort_order, ct);
    }
}
