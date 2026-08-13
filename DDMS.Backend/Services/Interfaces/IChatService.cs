using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Chat;

namespace DDMS.Backend.Services.Interfaces;

public interface IChatService
{
    Task<List<ConversationResponse>> GetConversationsAsync(Guid userId, CancellationToken ct);
    Task<List<MessageResponse>> GetMessagesAsync(Guid conversationId, Guid userId, int limit, DateTime? before, CancellationToken ct);
    Task<ConversationResponse> StartConversationAsync(Guid bookingId, Guid userId, CancellationToken ct);
    Task<MessageResponse> SendMessageAsync(
        Guid conversationId,
        Guid senderId,
        string? body,
        string? attachmentUrl,
        string? attachmentType,
        string? attachmentName,
        CancellationToken ct);
    Task<ChatAttachmentResponse> UploadAttachmentAsync(Guid conversationId, Guid userId, Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct);
    Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct);
}
