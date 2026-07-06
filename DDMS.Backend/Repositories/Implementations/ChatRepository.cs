using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.DTOs.Chat;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _db;

    public ChatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ConversationResponse>> GetConversationsByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var list = await _db.conversations
            .Where(c => c.conversation_members.Any(m => m.user_id == userId))
            .Select(c => new ConversationResponse
            {
                Id = c.id,
                Type = c.type,
                BookingId = c.booking_id,
                TourName = c.booking != null && c.booking.schedule != null && c.booking.schedule.tour != null 
                    ? c.booking.schedule.tour.name 
                    : null,
                PartnerName = c.conversation_members
                    .Where(m => m.user_id != userId)
                    .Select(m => m.user.full_name)
                    .FirstOrDefault() ?? "",
                PartnerAvatar = c.conversation_members
                    .Where(m => m.user_id != userId)
                    .Select(m => m.user.avatar_url)
                    .FirstOrDefault(),
                PartnerId = c.conversation_members
                    .Where(m => m.user_id != userId)
                    .Select(m => m.user_id)
                    .FirstOrDefault(),
                LastMessage = c.messages
                    .OrderByDescending(m => m.created_at)
                    .Select(m => m.body)
                    .FirstOrDefault(),
                LastMessageAt = c.messages
                    .OrderByDescending(m => m.created_at)
                    .Select(m => (DateTime?)m.created_at)
                    .FirstOrDefault(),
                UnreadCount = c.messages.Count(m =>
                    m.sender_id != userId &&
                    (c.conversation_members.Where(cm => cm.user_id == userId).Select(cm => cm.last_read_at).FirstOrDefault() == null ||
                     m.created_at > c.conversation_members.Where(cm => cm.user_id == userId).Select(cm => cm.last_read_at).FirstOrDefault())),
                CreatedAt = c.created_at,
                UpdatedAt = c.updated_at
            })
            .ToListAsync(ct);

        foreach (var conv in list)
        {
            if (conv.BookingId.HasValue)
            {
                conv.BookingCode = conv.BookingId.Value.ToString().Substring(0, 8).ToUpper();
            }
        }

        return list;
    }

    public async Task<List<message>> GetMessagesByConversationIdAsync(Guid conversationId, int limit, DateTime? before, CancellationToken ct)
    {
        var query = _db.messages
            .Include(m => m.sender)
            .Where(m => m.conversation_id == conversationId);

        if (before.HasValue)
        {
            query = query.Where(m => m.created_at < before.Value);
        }

        return await query
            .OrderByDescending(m => m.created_at)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken ct)
    {
        return await _db.conversations
            .Include(c => c.conversation_members)
            .FirstOrDefaultAsync(c => c.id == conversationId, ct);
    }

    public async Task<conversation?> GetConversationByBookingIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await _db.conversations
            .Include(c => c.conversation_members)
            .FirstOrDefaultAsync(c => c.booking_id == bookingId, ct);
    }

    public async Task AddConversationAsync(conversation conv, CancellationToken ct)
    {
        await _db.conversations.AddAsync(conv, ct);
    }

    public async Task AddConversationMemberAsync(conversation_member member, CancellationToken ct)
    {
        await _db.conversation_members.AddAsync(member, ct);
    }

    public async Task AddMessageAsync(message msg, CancellationToken ct)
    {
        await _db.messages.AddAsync(msg, ct);
    }

    public async Task<conversation_member?> GetMemberAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        return await _db.conversation_members
            .FirstOrDefaultAsync(m => m.conversation_id == conversationId && m.user_id == userId, ct);
    }

    public async Task<List<conversation_member>> GetConversationMembersAsync(Guid conversationId, CancellationToken ct)
    {
        return await _db.conversation_members
            .Include(m => m.user)
            .Where(m => m.conversation_id == conversationId)
            .ToListAsync(ct);
    }

    public async Task<booking?> GetBookingWithTourAndOwnerAsync(Guid bookingId, CancellationToken ct)
    {
        return await _db.bookings
            .Include(b => b.user)
            .Include(b => b.schedule)
                .ThenInclude(s => s.tour)
                    .ThenInclude(t => t.created_byNavigation)
            .FirstOrDefaultAsync(b => b.id == bookingId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
