using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.DTOs.Ai;

public class AiChatRequestDto
{
    public Guid? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AiRecommendedTourDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Duration { get; set; }
    public string? DepartureLocation { get; set; }
}

public class AiChatResponseDto
{
    public Guid ConversationId { get; set; }
    public string Reply { get; set; } = string.Empty;
    public List<AiRecommendedTourDto> RecommendedTours { get; set; } = new();
}

public class AiConversationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class AiMessageDto
{
    public Guid Id { get; set; }
    public string SenderRole { get; set; } = string.Empty; // "user" | "model" / "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
