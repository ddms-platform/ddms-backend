using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class CertificateTypeRepository : ICertificateTypeRepository
{
    private readonly AppDbContext _db;

    public CertificateTypeRepository(AppDbContext db) => _db = db;

    public Task<List<certificate_type>> GetAllAsync(CancellationToken ct = default) =>
        _db.certificate_types
            .OrderBy(t => t.sort_order)
            .ThenBy(t => t.id)
            .ToListAsync(ct);

    public Task<List<certificate_type>> GetActiveAsync(CancellationToken ct = default) =>
        _db.certificate_types
            .Where(t => t.is_active)
            .OrderBy(t => t.sort_order)
            .ThenBy(t => t.id)
            .ToListAsync(ct);

    public Task<certificate_type?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.certificate_types.FirstOrDefaultAsync(t => t.id == id, ct);

    public Task<certificate_type?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        _db.certificate_types.FirstOrDefaultAsync(t => t.code == code, ct);

    public Task<bool> ExistsActiveCodeAsync(string code, CancellationToken ct = default) =>
        _db.certificate_types.AnyAsync(t => t.code == code && t.is_active, ct);

    public Task<bool> IsCodeInUseAsync(string code, CancellationToken ct = default) =>
        _db.boat_certificates.AnyAsync(c => c.certificate_type == code, ct);

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
