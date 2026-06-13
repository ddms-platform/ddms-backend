namespace DDMS.Backend.Models.DTOs.Tours;

public class TourItemResponse
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public int durationMinutes { get; set; }
    public string? location { get; set; }
    public string status { get; set; } = string.Empty;
    public string cancelPolicy { get; set; } = string.Empty;
    public int? cancelHours { get; set; }
    public decimal avgRating { get; set; }
    public int totalReviews { get; set; }
    public string? mapUrl { get; set; }
    public List<TourRouteResponse> routes { get; set; } = new();
    public List<TourFaqResponse> faqs { get; set; } = new();
    public List<TourClassResponse> classes { get; set; } = new();
    public List<TourServiceResponse> services { get; set; } = new();
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}
