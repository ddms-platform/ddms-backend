using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class OwnerDocumentRepository : IOwnerDocumentRepository
{
    private readonly AppDbContext _db;

    public OwnerDocumentRepository(AppDbContext db) => _db = db;

    public Task<List<owner_document>> GetByProfileIdAsync(Guid ownerProfileId, CancellationToken ct = default) =>
        _db.owner_documents
            .Where(d => d.owner_profile_id == ownerProfileId)
            .OrderBy(d => d.document_type)
            .ToListAsync(ct);

    public Task<owner_document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.owner_documents.FirstOrDefaultAsync(d => d.id == id, ct);

    public Task<owner_document?> GetByProfileAndTypeAsync(
        Guid ownerProfileId, string documentType, CancellationToken ct = default) =>
        _db.owner_documents
            .FirstOrDefaultAsync(d => d.owner_profile_id == ownerProfileId && d.document_type == documentType, ct);

    public Task<owner_profile?> GetProfileByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        _db.owner_profiles.FirstOrDefaultAsync(p => p.user_id == userId, ct);

    public Task<List<owner_profile>> GetVerifiedProfilesWithDocumentsAsync(CancellationToken ct = default) =>
        _db.owner_profiles
            .Include(p => p.owner_documents)
            .Include(p => p.user)
            .Where(p => p.status == "verified" || p.is_verified == true)
            .ToListAsync(ct);

    public void Add(owner_document entity) => _db.owner_documents.Add(entity);

    public async Task<owner_document> AddAsync(owner_document entity, CancellationToken ct = default)
    {
        _db.owner_documents.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(owner_document entity, CancellationToken ct = default)
    {
        _db.owner_documents.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
