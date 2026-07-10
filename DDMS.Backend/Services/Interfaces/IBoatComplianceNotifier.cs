using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Services.Interfaces;

public interface IBoatComplianceNotifier
{
    Task NotifyCertificateExpiringSoonAsync(boat_certificate certificate, CancellationToken ct = default);
    Task NotifyCertificateExpiredHiddenAsync(boat boat, CancellationToken ct = default);
    Task NotifyCertificateLockedAsync(boat boat, CancellationToken ct = default);
    Task NotifyCertificateApprovedAsync(boat_certificate certificate, CancellationToken ct = default);
    Task NotifyCertificateRejectedAsync(boat_certificate certificate, CancellationToken ct = default);
}
