namespace DDMS.Backend.Models.DTOs.Tours;

public class TourImageItemResponse
{
    public Guid id { get; set; }
    public Guid tourId { get; set; }
    public string imageUrl { get; set; } = string.Empty;
    public string? publicId { get; set; }
    public string? caption { get; set; }
    public int sortOrder { get; set; }
    public DateTime createdAt { get; set; }
}
