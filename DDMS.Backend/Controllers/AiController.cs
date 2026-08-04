using System;
using System.Threading.Tasks;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Models.DTOs.Ai;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ICurrentUser _currentUser;
    private static readonly Guid GuestUserId = new Guid("00000000-0000-0000-0000-000000000001");

    public AiController(IAiService aiService, ICurrentUser currentUser)
    {
        _aiService = aiService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Send a message to AI Assistant Chatbot
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        var userId = _currentUser.IdOrNull ?? GuestUserId;
        var response = await _aiService.SendMessageAsync(userId, request);
        return Ok(response);
    }

    /// <summary>
    /// Get conversation history for current user
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = _currentUser.IdOrNull ?? GuestUserId;
        var conversations = await _aiService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    /// <summary>
    /// Get messages in a specific conversation
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        var userId = _currentUser.IdOrNull ?? GuestUserId;
        var messages = await _aiService.GetConversationMessagesAsync(userId, conversationId);
        return Ok(messages);
    }

    /// <summary>
    /// Delete an AI conversation
    /// </summary>
    [HttpDelete("conversations/{conversationId:guid}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var userId = _currentUser.IdOrNull ?? GuestUserId;
        var success = await _aiService.DeleteConversationAsync(userId, conversationId);
        if (!success)
        {
            return NotFound(new { message = "Conversation not found." });
        }
        return Ok(new { message = "Conversation deleted successfully." });
    }
}
