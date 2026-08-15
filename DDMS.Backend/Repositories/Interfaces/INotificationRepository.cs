using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface INotificationRepository
{
    Task AddNotificationAsync(notification notif, CancellationToken ct);
    Task AddRecipientsAsync(IEnumerable<notification_recipient> recipients, CancellationToken ct);
    Task<List<notification_recipient>> GetUserNotificationsAsync(Guid userId, int limit, CancellationToken ct);
    Task<notification_recipient?> GetRecipientRecordAsync(Guid id, Guid userId, CancellationToken ct);
    Task<List<notification_recipient>> GetUnreadRecipientRecordsAsync(Guid userId, CancellationToken ct);
    Task<List<Guid>> GetAdminUserIdsAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
