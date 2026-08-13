using DDMS.Backend.Hubs;
using DDMS.Backend.Models.DTOs.AdminOps;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Services.Implementations;

public class AdminAlertPublisher : IAdminAlertPublisher
{
    private readonly IHubContext<AdminAlertsHub> _hub;

    public AdminAlertPublisher(IHubContext<AdminAlertsHub> hub)
    {
        _hub = hub;
    }

    public async Task PublishAsync(AlertItem alert, CancellationToken ct = default)
    {
        var payload = new
        {
            severity = alert.Severity,
            title = alert.Title,
            detail = alert.Detail,
            createdAt = DateTime.UtcNow,
        };
        await _hub.Clients.Group(AdminAlertsHub.GroupName).SendAsync("ReceiveAlert", payload, ct);
    }
}
