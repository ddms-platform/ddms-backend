using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Hubs;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IHubContext<ChatHub> _hubContext;

    public NotificationService(INotificationRepository repo, IHubContext<ChatHub> hubContext)
    {
        _repo = repo;
        _hubContext = hubContext;
    }

    public async Task CreateNotificationAsync(Guid? senderId, string type, string title, string body, List<Guid> recipientIds, string? data, CancellationToken ct)
    {
        if (recipientIds == null || !recipientIds.Any()) return;

        var notif = new notification
        {
            id = Guid.NewGuid(),
            sender_id = senderId,
            type = type,
            title = title,
            body = body,
            data = data,
            created_at = DateTime.UtcNow
        };

        await _repo.AddNotificationAsync(notif, ct);

        var now = DateTime.UtcNow;
        var recipients = recipientIds.Select(userId => new notification_recipient
        {
            id = Guid.NewGuid(),
            notification_id = notif.id,
            user_id = userId,
            is_read = false,
            created_at = now
        }).ToList();

        await _repo.AddRecipientsAsync(recipients, ct);
        await _repo.SaveChangesAsync(ct);

        // Send real-time SignalR notifications to all online recipients
        foreach (var recipient in recipients)
        {
            try
            {
                await _hubContext.Clients.Group(recipient.user_id.ToString()).SendAsync("ReceiveNotification", new
                {
                    id = recipient.id,
                    title = notif.title,
                    content = notif.body,
                    createdAt = recipient.created_at.ToString("o"), // ISO 8601
                    isRead = recipient.is_read
                });
            }
            catch
            {
                // Suppress SignalR delivery errors so the DB transaction remains robust
            }
        }
    }

    public Task<List<notification_recipient>> GetUserNotificationsAsync(Guid userId, int limit, CancellationToken ct)
    {
        return _repo.GetUserNotificationsAsync(userId, limit, ct);
    }

    public async Task MarkAsReadAsync(Guid recipientRecordId, Guid userId, CancellationToken ct)
    {
        var record = await _repo.GetRecipientRecordAsync(recipientRecordId, userId, ct);
        if (record == null || record.is_read) return;

        record.is_read = true;
        record.read_at = DateTime.UtcNow;

        await _repo.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct)
    {
        var unreadRecords = await _repo.GetUnreadRecipientRecordsAsync(userId, ct);
        if (!unreadRecords.Any()) return;

        var now = DateTime.UtcNow;
        foreach (var record in unreadRecords)
        {
            record.is_read = true;
            record.read_at = now;
        }

        await _repo.SaveChangesAsync(ct);
    }
}
