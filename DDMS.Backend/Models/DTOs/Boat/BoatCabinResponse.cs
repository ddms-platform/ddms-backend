namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatCabinResponse
{
    public Guid id { get; init; }
    public Guid boatId { get; init; }
    public string name { get; init; } = null!;
    public int capacity { get; init; }
    public decimal price { get; init; }
    public int totalRooms { get; init; }
    public string? description { get; init; }
    public DateTime createdAt { get; init; }
    public DateTime updatedAt { get; init; }
}
