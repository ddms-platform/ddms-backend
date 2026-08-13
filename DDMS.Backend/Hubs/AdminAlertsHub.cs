using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Hubs;

/// <summary>
/// SignalR hub for pushing real-time ops alerts to admin users.
/// All admins join the shared "admins" group on connect.
/// </summary>
[Authorize(Roles = "admin")]
public class AdminAlertsHub : Hub
{
    public const string GroupName = "admins";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
        await base.OnDisconnectedAsync(exception);
    }
}
