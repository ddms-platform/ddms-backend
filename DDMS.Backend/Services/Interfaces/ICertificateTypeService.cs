using DDMS.Backend.Models.DTOs.BoatCertificate;

namespace DDMS.Backend.Services.Interfaces;

public interface ICertificateTypeService
{
    Task<List<CertificateTypeItem>> GetActiveAsync(CancellationToken ct = default);
    Task<List<CertificateTypeItem>> GetAllForAdminAsync(CancellationToken ct = default);
    Task<CertificateTypeItem> CreateAsync(CreateCertificateTypeRequest request, CancellationToken ct = default);
    Task<CertificateTypeItem> UpdateAsync(int id, UpdateCertificateTypeRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task EnsureActiveCodeAsync(string code, CancellationToken ct = default);
}
