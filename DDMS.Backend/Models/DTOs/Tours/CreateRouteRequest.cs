namespace DDMS.Backend.Models.DTOs.Tours;

public class CreateRouteRequest
{
    public Guid tourId { get; set; }
    public string? name { get; set; }
    public string startPoint { get; set; } = string.Empty;
    public string endPoint { get; set; } = string.Empty;
    public string? description { get; set; }
    public int sortOrder { get; set; }
}
