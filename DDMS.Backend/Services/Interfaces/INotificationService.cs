using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Services.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid? senderId, string type, string title, string body, List<Guid> recipientIds, string? data, CancellationToken ct);
    Task<List<notification_recipient>> GetUserNotificationsAsync(Guid userId, int limit, CancellationToken ct);
    Task MarkAsReadAsync(Guid recipientRecordId, Guid userId, CancellationToken ct);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct);
}
