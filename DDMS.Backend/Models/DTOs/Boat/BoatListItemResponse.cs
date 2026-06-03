namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatListItemResponse
{
    public Guid id { get; init; }
    public Guid? ownerId { get; init; }
    public string name { get; init; } = null!;
    public string? type { get; init; }
    public int maxPassengers { get; init; }
    public string status { get; init; } = null!;
    public int cabinCount { get; init; }
    public int serviceCount { get; init; }
    public string? thumbnailUrl { get; init; }
    public DateTime createdAt { get; init; }
    public DateTime updatedAt { get; init; }
}
