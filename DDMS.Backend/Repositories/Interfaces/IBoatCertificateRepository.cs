using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBoatCertificateRepository
{
    Task<List<boat_certificate>> GetByBoatIdAsync(Guid boatId, CancellationToken ct = default);
    Task<boat_certificate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<boat_certificate?> GetByBoatAndTypeAsync(Guid boatId, string certificateType, CancellationToken ct = default);
    Task<boat_certificate> AddAsync(boat_certificate entity, CancellationToken ct = default);
    Task UpdateAsync(boat_certificate entity, CancellationToken ct = default);
    Task<List<boat_certificate>> GetPendingForAdminAsync(CancellationToken ct = default);
    Task<List<boat_certificate>> GetExpiringAsync(DateOnly expiryThreshold, CancellationToken ct = default);
    Task<int> MarkExpiredAsync(DateOnly today, CancellationToken ct = default);
    Task SetBoatComplianceStatusAsync(Guid boatId, string complianceStatus, CancellationToken ct = default);
    Task<int> DeactivateBoatServicesAsync(Guid boatId, CancellationToken ct = default);
    Task<boat?> GetBoatByIdAsync(Guid boatId, CancellationToken ct = default);
    Task<List<boat>> GetBoatsWithCertificatesForComplianceAsync(CancellationToken ct = default);
    Task<List<boat_certificate>> GetCertificatesNeedingReminderAsync(
        DateOnly today, DateOnly warningThreshold, CancellationToken ct = default);
    Task MarkReminderSentAsync(Guid certId, CancellationToken ct = default);
}
