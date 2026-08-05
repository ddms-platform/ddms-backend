using System;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Sos;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Hubs;

public class SosHub : Hub
{
    public const string AdminGroup = "Admins";
    public const string CaptainsGroup = "Captains";

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
    }

    public async Task JoinCaptainGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, CaptainsGroup);
    }

    public async Task SendSosAlert(SosAlertResponse alert)
    {
        await Clients.Group(AdminGroup).SendAsync("ReceiveSosAlert", alert);
    }

    public async Task ResolveSosAlert(Guid sosId)
    {
        await Clients.All.SendAsync("SosAlertResolved", sosId);
    }
}
