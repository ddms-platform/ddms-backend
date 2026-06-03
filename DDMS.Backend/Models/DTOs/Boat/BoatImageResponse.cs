namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatImageResponse
{
    public Guid id { get; init; }
    public Guid boatId { get; init; }
    public string imageUrl { get; init; } = null!;
    public string? publicId { get; init; }
    public string? caption { get; init; }
    public int sortOrder { get; init; }
    public DateTime createdAt { get; init; }
}
