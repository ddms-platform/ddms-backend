using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Hubs;
using DDMS.Backend.Models.DTOs.Chat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DDMS.Backend.Services.Implementations;

public class ChatService : IChatService
{
    private readonly IChatRepository _repo;
    private readonly IEmailSender _emailSender;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(IChatRepository repo, IEmailSender emailSender, IHubContext<ChatHub> hubContext)
    {
        _repo = repo;
        _emailSender = emailSender;
        _hubContext = hubContext;
    }

    public async Task<List<ConversationResponse>> GetConversationsAsync(Guid userId, CancellationToken ct)
    {
        var conversations = await _repo.GetConversationsByUserIdAsync(userId, ct);
        return conversations.OrderByDescending(r => r.LastMessageAt ?? r.UpdatedAt).ToList();
    }

    public async Task<List<MessageResponse>> GetMessagesAsync(Guid conversationId, Guid userId, int limit, DateTime? before, CancellationToken ct)
    {
        var member = await _repo.GetMemberAsync(conversationId, userId, ct);
        if (member == null)
        {
            throw new ForbiddenException("Bạn không phải thành viên của cuộc hội thoại này.");
        }

        var messages = await _repo.GetMessagesByConversationIdAsync(conversationId, limit, before, ct);

        return messages.Select(m => new MessageResponse
        {
            Id = m.id,
            ConversationId = m.conversation_id,
            SenderId = m.sender_id,
            SenderName = m.sender.full_name,
            SenderAvatar = m.sender.avatar_url,
            Body = m.body,
            CreatedAt = m.created_at
        }).OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task<ConversationResponse> StartConversationAsync(Guid bookingId, Guid userId, CancellationToken ct)
    {
        var booking = await _repo.GetBookingWithTourAndOwnerAsync(bookingId, ct);
        if (booking == null)
        {
            throw new NotFoundException("Không tìm thấy đơn đặt lịch này.");
        }

        var ownerId = booking.schedule.tour.created_by;
        if (booking.user_id != userId && ownerId != userId)
        {
            throw new ForbiddenException("Bạn không có quyền bắt đầu cuộc hội thoại cho đơn đặt lịch này.");
        }

        var existingConv = await _repo.GetConversationByBookingIdAsync(bookingId, ct);
        if (existingConv != null)
        {
            var resultList = await GetConversationsAsync(userId, ct);
            var matched = resultList.FirstOrDefault(c => c.Id == existingConv.id);
            if (matched != null)
                return matched;
        }

        var newConv = new conversation
        {
            id = Guid.NewGuid(),
            type = "direct",
            booking_id = bookingId,
            created_by = userId,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _repo.AddConversationAsync(newConv, ct);

        // Add Customer member
        await _repo.AddConversationMemberAsync(new conversation_member
        {
            id = Guid.NewGuid(),
            conversation_id = newConv.id,
            user_id = booking.user_id,
            joined_at = DateTime.UtcNow,
            last_read_at = (booking.user_id == userId) ? DateTime.UtcNow : null
        }, ct);

        // Add Owner member
        if (ownerId.HasValue)
        {
            await _repo.AddConversationMemberAsync(new conversation_member
            {
                id = Guid.NewGuid(),
                conversation_id = newConv.id,
                user_id = ownerId.Value,
                joined_at = DateTime.UtcNow,
                last_read_at = (ownerId.Value == userId) ? DateTime.UtcNow : null
            }, ct);
        }

        await _repo.SaveChangesAsync(ct);

        // Refresh and return
        var list = await GetConversationsAsync(userId, ct);
        return list.First(c => c.Id == newConv.id);
    }

    public async Task<MessageResponse> SendMessageAsync(Guid conversationId, Guid senderId, string body, CancellationToken ct)
    {
        var conversation = await _repo.GetConversationByIdAsync(conversationId, ct);
        if (conversation == null)
        {
            throw new NotFoundException("Cuộc hội thoại không tồn tại.");
        }

        var members = await _repo.GetConversationMembersAsync(conversationId, ct);
        var senderMember = members.FirstOrDefault(m => m.user_id == senderId);
        if (senderMember == null)
        {
            throw new ForbiddenException("Bạn không phải thành viên của cuộc hội thoại này.");
        }

        var msg = new message
        {
            id = Guid.NewGuid(),
            conversation_id = conversationId,
            sender_id = senderId,
            body = body,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _repo.AddMessageAsync(msg, ct);

        // Update conversation updated_at
        conversation.updated_at = DateTime.UtcNow;

        // Update sender's last_read_at
        senderMember.last_read_at = DateTime.UtcNow;

        await _repo.SaveChangesAsync(ct);

        var response = new MessageResponse
        {
            Id = msg.id,
            ConversationId = msg.conversation_id,
            SenderId = msg.sender_id,
            SenderName = senderMember.user.full_name,
            SenderAvatar = senderMember.user.avatar_url,
            Body = msg.body,
            CreatedAt = msg.created_at
        };

        // Realtime broadcast to all conversation members
        foreach (var m in members)
        {
            await _hubContext.Clients.Group(m.user_id.ToString()).SendAsync("ReceiveMessage", response, cancellationToken: ct);
        }

        // Email notification trigger for offline members
        var offlineMembers = members.Where(m => m.user_id != senderId && !ChatHub.IsUserOnline(m.user_id)).ToList();
        foreach (var om in offlineMembers)
        {
            var recipientEmail = om.user.email;
            var recipientName = om.user.full_name;
            var senderName = senderMember.user.full_name;
            var viewChatLink = $"http://localhost:5173/inbox?conversationId={conversationId}";

            // Run in a background thread to prevent blocking the HTTP response
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailSender.SendNewChatMessageEmailAsync(recipientEmail, recipientName, senderName, body, viewChatLink);
                }
                catch (Exception)
                {
                    // Ignore email sending failures in background thread
                }
            }, CancellationToken.None);
        }

        return response;
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var member = await _repo.GetMemberAsync(conversationId, userId, ct);
        if (member != null)
        {
            member.last_read_at = DateTime.UtcNow;
            await _repo.SaveChangesAsync(ct);
        }
    }
}
