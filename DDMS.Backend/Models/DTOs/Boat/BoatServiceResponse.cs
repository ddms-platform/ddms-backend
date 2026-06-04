namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatServiceResponse
{
    public Guid id { get; init; }
    public Guid boatId { get; init; }
    public string name { get; init; } = null!;
    public decimal price { get; init; }
    public string? description { get; init; }
    public bool isActive { get; init; }
    public DateTime createdAt { get; init; }
    public DateTime updatedAt { get; init; }
}
