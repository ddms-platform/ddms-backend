using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddNotificationAsync(notification notif, CancellationToken ct)
    {
        await _db.notifications.AddAsync(notif, ct);
    }

    public async Task AddRecipientsAsync(IEnumerable<notification_recipient> recipients, CancellationToken ct)
    {
        await _db.notification_recipients.AddRangeAsync(recipients, ct);
    }

    public Task<List<notification_recipient>> GetUserNotificationsAsync(Guid userId, int limit, CancellationToken ct)
    {
        return _db.notification_recipients
            .Include(nr => nr.notification)
            .Where(nr => nr.user_id == userId)
            .OrderByDescending(nr => nr.created_at)
            .Take(limit)
            .ToListAsync(ct);
    }

    public Task<notification_recipient?> GetRecipientRecordAsync(Guid id, Guid userId, CancellationToken ct)
    {
        return _db.notification_recipients
            .FirstOrDefaultAsync(nr => nr.id == id && nr.user_id == userId, ct);
    }

    public Task<List<notification_recipient>> GetUnreadRecipientRecordsAsync(Guid userId, CancellationToken ct)
    {
        return _db.notification_recipients
            .Where(nr => nr.user_id == userId && !nr.is_read)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
