using System;

namespace DDMS.Backend.Models.DTOs.Chat;

public class ConversationResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public Guid? BookingId { get; set; }
    public string? BookingCode { get; set; }
    public string? TourName { get; set; }
    public string PartnerName { get; set; } = null!;
    public string? PartnerAvatar { get; set; }
    public Guid PartnerId { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string? SenderAvatar { get; set; }
    public string Body { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public string? AttachmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatAttachmentResponse
{
    public string Url { get; set; } = null!;
    public string Type { get; set; } = null!; // "image" | "video"
    public string Name { get; set; } = null!;
}

public class StartConversationRequest
{
    public Guid BookingId { get; set; }
}

public class SendMessageRequest
{
    public string? Body { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public string? AttachmentName { get; set; }
}
