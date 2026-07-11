using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ICertificateTypeRepository
{
    Task<List<certificate_type>> GetAllAsync(CancellationToken ct = default);
    Task<List<certificate_type>> GetActiveAsync(CancellationToken ct = default);
    Task<certificate_type?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<certificate_type?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsActiveCodeAsync(string code, CancellationToken ct = default);
    Task<bool> IsCodeInUseAsync(string code, CancellationToken ct = default);
    Task<certificate_type> AddAsync(certificate_type entity, CancellationToken ct = default);
    Task UpdateAsync(certificate_type entity, CancellationToken ct = default);
    Task DeleteAsync(certificate_type entity, CancellationToken ct = default);
    Task<int> GetMaxSortOrderAsync(CancellationToken ct = default);
}
