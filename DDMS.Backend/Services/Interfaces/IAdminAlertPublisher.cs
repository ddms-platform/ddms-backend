using DDMS.Backend.Models.DTOs.AdminOps;

namespace DDMS.Backend.Services.Interfaces;

/// <summary>
/// Publishes real-time alerts to all connected admins via SignalR.
/// Domain services call this whenever a noteworthy event occurs.
/// </summary>
public interface IAdminAlertPublisher
{
    Task PublishAsync(AlertItem alert, CancellationToken ct = default);
}
