using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUser _user;

    public NotificationsController(INotificationService notificationService, ICurrentUser user)
    {
        _notificationService = notificationService;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var userId = _user.IdOrNull;
        if (userId is null)
        {
            return Unauthorized();
        }

        var list = await _notificationService.GetUserNotificationsAsync(userId.Value, limit, ct);
        var response = list.Select(record => new
        {
            id = record.id,
            title = record.notification?.title ?? "Thông báo",
            content = record.notification?.body ?? "",
            createdAt = record.created_at.ToString("o"),
            isRead = record.is_read
        }).ToList();

        return Ok(response);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var userId = _user.IdOrNull;
        if (userId is null)
        {
            return Unauthorized();
        }

        await _notificationService.MarkAsReadAsync(id, userId.Value, ct);
        return Ok(new { success = true });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = _user.IdOrNull;
        if (userId is null)
        {
            return Unauthorized();
        }

        await _notificationService.MarkAllAsReadAsync(userId.Value, ct);
        return Ok(new { success = true });
    }
}
