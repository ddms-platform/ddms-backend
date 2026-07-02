using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.DTOs.Chat;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IChatRepository
{
    Task<List<ConversationResponse>> GetConversationsByUserIdAsync(Guid userId, CancellationToken ct);
    Task<List<message>> GetMessagesByConversationIdAsync(Guid conversationId, int limit, DateTime? before, CancellationToken ct);
    Task<conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken ct);
    Task<conversation?> GetConversationByBookingIdAsync(Guid bookingId, CancellationToken ct);
    Task AddConversationAsync(conversation conv, CancellationToken ct);
    Task AddConversationMemberAsync(conversation_member member, CancellationToken ct);
    Task AddMessageAsync(message msg, CancellationToken ct);
    Task<conversation_member?> GetMemberAsync(Guid conversationId, Guid userId, CancellationToken ct);
    Task<List<conversation_member>> GetConversationMembersAsync(Guid conversationId, CancellationToken ct);
    Task<booking?> GetBookingWithTourAndOwnerAsync(Guid bookingId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
