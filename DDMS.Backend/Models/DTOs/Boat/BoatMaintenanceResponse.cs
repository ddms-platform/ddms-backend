namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatMaintenanceResponse
{
    public Guid id { get; init; }
    public Guid boatId { get; init; }
    public DateTime startTime { get; init; }
    public DateTime endTime { get; init; }
    public string? reason { get; init; }
    public DateTime createdAt { get; init; }
}
