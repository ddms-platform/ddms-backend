using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Chat;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ICurrentUser _currentUser;

    public ChatController(IChatService chatService, ICurrentUser currentUser)
    {
        _chatService = chatService;
        _currentUser = currentUser;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var result = await _chatService.GetConversationsAsync(_currentUser.Id, ct);
        return Ok(ApiResponse<List<ConversationResponse>>.Ok(result));
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid id,
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? before = null,
        CancellationToken ct = default)
    {
        var result = await _chatService.GetMessagesAsync(id, _currentUser.Id, limit, before, ct);
        return Ok(ApiResponse<List<MessageResponse>>.Ok(result));
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken ct)
    {
        var result = await _chatService.StartConversationAsync(request.BookingId, _currentUser.Id, ct);
        return Ok(ApiResponse<ConversationResponse>.Ok(result));
    }

    [HttpPost("conversations/{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var result = await _chatService.SendMessageAsync(
            id,
            _currentUser.Id,
            request.Body,
            request.AttachmentUrl,
            request.AttachmentType,
            request.AttachmentName,
            ct);
        return Ok(ApiResponse<MessageResponse>.Ok(result));
    }

    [HttpPost("conversations/{id:guid}/attachments")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await _chatService.UploadAttachmentAsync(id, _currentUser.Id, file, ct);
        return Ok(ApiResponse<ChatAttachmentResponse>.Ok(result));
    }

    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        await _chatService.MarkAsReadAsync(id, _currentUser.Id, ct);
        return Ok(ApiResponse<string>.Ok("Success"));
    }
}
