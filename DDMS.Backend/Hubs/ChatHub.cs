using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> OnlineUsers = new();

    public static bool IsUserOnline(Guid userId)
    {
        return OnlineUsers.TryGetValue(userId, out var connections) && connections.Count > 0;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            OnlineUsers.AddOrUpdate(userId,
                _ => new HashSet<string> { Context.ConnectionId },
                (_, hs) =>
                {
                    lock (hs)
                    {
                        hs.Add(Context.ConnectionId);
                    }
                    return hs;
                });

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            if (OnlineUsers.TryGetValue(userId, out var hs))
            {
                lock (hs)
                {
                    hs.Remove(Context.ConnectionId);
                    if (hs.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                    }
                }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    private Guid GetUserId()
    {
        var raw = Context.User?.FindFirst("sub")?.Value
               ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
