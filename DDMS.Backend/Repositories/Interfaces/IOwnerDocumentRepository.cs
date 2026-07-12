using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IOwnerDocumentRepository
{
    Task<List<owner_document>> GetByProfileIdAsync(Guid ownerProfileId, CancellationToken ct = default);
    Task<owner_document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<owner_document?> GetByProfileAndTypeAsync(Guid ownerProfileId, string documentType, CancellationToken ct = default);
    Task<owner_profile?> GetProfileByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Add(owner_document entity);
    Task<owner_document> AddAsync(owner_document entity, CancellationToken ct = default);
    Task UpdateAsync(owner_document entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
