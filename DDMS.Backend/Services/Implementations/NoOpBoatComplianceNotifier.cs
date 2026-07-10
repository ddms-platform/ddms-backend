using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class NoOpBoatComplianceNotifier : IBoatComplianceNotifier
{
    public Task NotifyCertificateExpiringSoonAsync(boat_certificate certificate, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyCertificateExpiredHiddenAsync(boat boat, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyCertificateLockedAsync(boat boat, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyCertificateApprovedAsync(boat_certificate certificate, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task NotifyCertificateRejectedAsync(boat_certificate certificate, CancellationToken ct = default) =>
        Task.CompletedTask;
}
