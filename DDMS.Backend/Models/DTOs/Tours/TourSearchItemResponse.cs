namespace DDMS.Backend.Models.DTOs.Tours;

public class TourSearchItemResponse
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public int durationMinutes { get; set; }
    public string? location { get; set; }
    public string status { get; set; } = string.Empty;
    public decimal avgRating { get; set; }
    public int totalReviews { get; set; }
    public string cancelPolicy { get; set; } = string.Empty;
    public int? cancelHours { get; set; }
    public List<AvailableSlotResponse> availableSlots { get; set; } = [];
}
