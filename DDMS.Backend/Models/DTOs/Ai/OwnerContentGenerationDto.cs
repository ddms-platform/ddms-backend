namespace DDMS.Backend.Models.DTOs.Ai;

/// <summary>
/// Request from Owner Content Studio — asks AI to generate one piece of tour content.
/// </summary>
public class OwnerContentRequestDto
{
    /// <summary>"name" | "description" | "faqs" | "price"</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Owner-provided keywords/context (e.g. "sông Hàn, hoàng hôn, gia đình, 2h").</summary>
    public string Keywords { get; set; } = string.Empty;

    /// <summary>Optional: current tour name (helps generate related content like FAQs).</summary>
    public string? TourName { get; set; }

    /// <summary>Optional: current description (used when generating FAQs).</summary>
    public string? Description { get; set; }

    /// <summary>Optional: service type (cruise/dinner/fishing/speedboat/complex_tour).</summary>
    public string? ServiceType { get; set; }

    /// <summary>Optional: duration in minutes (for pricing context).</summary>
    public int? DurationMinutes { get; set; }
}

public class OwnerContentResponseDto
{
    public string Type { get; set; } = string.Empty;

    /// <summary>Single-string result (description, price rationale).</summary>
    public string? Text { get; set; }

    /// <summary>Multiple options (tour name suggestions).</summary>
    public List<string>? Options { get; set; }

    /// <summary>Structured FAQs.</summary>
    public List<FaqItem>? Faqs { get; set; }

    /// <summary>Suggested numeric price (VND).</summary>
    public decimal? SuggestedPrice { get; set; }
}

public class FaqItem
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
