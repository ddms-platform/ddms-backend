using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ICertificateTypeRepository
{
    Task<List<certificate_type>> GetAllAsync(string? scope = null, CancellationToken ct = default);
    Task<List<certificate_type>> GetActiveAsync(string? scope = null, CancellationToken ct = default);
    Task<certificate_type?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<certificate_type?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsActiveCodeAsync(string code, string scope, CancellationToken ct = default);
    Task<bool> IsCodeInUseAsync(string code, CancellationToken ct = default);
    Task<certificate_type> AddAsync(certificate_type entity, CancellationToken ct = default);
    Task UpdateAsync(certificate_type entity, CancellationToken ct = default);
    Task DeleteAsync(certificate_type entity, CancellationToken ct = default);
    Task<int> GetMaxSortOrderAsync(CancellationToken ct = default);
}
