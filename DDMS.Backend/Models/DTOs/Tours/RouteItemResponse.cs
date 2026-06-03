namespace DDMS.Backend.Models.DTOs.Tours;

public class RouteItemResponse
{
    public Guid id { get; set; }
    public Guid tourId { get; set; }
    public string tourName { get; set; } = string.Empty;
    public string? name { get; set; }
    public string startPoint { get; set; } = string.Empty;
    public string endPoint { get; set; } = string.Empty;
    public string? description { get; set; }
    public int sortOrder { get; set; }
    public DateTime createdAt { get; set; }
}
