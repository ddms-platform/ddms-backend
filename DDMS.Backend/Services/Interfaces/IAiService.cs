using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Ai;

namespace DDMS.Backend.Services.Interfaces;

public interface IAiService
{
    Task<AiChatResponseDto> SendMessageAsync(Guid userId, AiChatRequestDto request);
    Task<List<AiConversationDto>> GetUserConversationsAsync(Guid userId);
    Task<List<AiMessageDto>> GetConversationMessagesAsync(Guid userId, Guid conversationId);
    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId);
}
