using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IPromotionsRepository
{
    Task<List<promotion>> GetAllWithCreatorAsync(CancellationToken ct);
    Task<List<promotion>> GetByOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<promotion?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    void Add(promotion entity);
    void Remove(promotion entity);
    Task SaveChangesAsync(CancellationToken ct);
}
