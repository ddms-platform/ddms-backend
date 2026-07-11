using DDMS.Backend.Models.DTOs.BoatCertificate;

namespace DDMS.Backend.Services.Interfaces;

public interface IBoatCertificateService
{
    Task<List<CertificateResponse>> GetByBoatIdForOwnerAsync(Guid boatId, Guid ownerId, CancellationToken ct = default);
    Task<List<CertificateListItem>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<CertificateResponse> UploadAsync(Guid boatId, Guid ownerId, UploadCertificateRequest request, CancellationToken ct = default);
    Task<CertificateResponse> RenewAsync(Guid boatId, Guid certId, Guid ownerId, RenewCertificateRequest request, CancellationToken ct = default);
    Task<List<CertificateListItem>> GetPendingForAdminAsync(CancellationToken ct = default);
    Task<List<CertificateListItem>> GetExpiringForAdminAsync(CancellationToken ct = default);
    Task ApproveAsync(Guid certId, Guid adminId, CancellationToken ct = default);
    Task RejectAsync(Guid certId, Guid adminId, RejectCertificateRequest request, CancellationToken ct = default);
    Task UnlockBoatAsync(Guid boatId, Guid adminId, CancellationToken ct = default);
}
