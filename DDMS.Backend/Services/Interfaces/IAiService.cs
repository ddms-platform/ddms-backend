using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Ai;

namespace DDMS.Backend.Services.Interfaces;

public interface IAiService
{
    Task<AiChatResponseDto> SendMessageAsync(Guid userId, AiChatRequestDto request);

    /// <summary>Streaming version — yields text chunks as they arrive from Gemini, then final envelope with tours.</summary>
    IAsyncEnumerable<AiStreamChunk> SendMessageStreamAsync(Guid userId, AiChatRequestDto request, CancellationToken ct);

    Task<List<AiConversationDto>> GetUserConversationsAsync(Guid userId);
    Task<List<AiMessageDto>> GetConversationMessagesAsync(Guid userId, Guid conversationId);
    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId);
    Task<OwnerContentResponseDto> GenerateOwnerContentAsync(OwnerContentRequestDto request);
}
