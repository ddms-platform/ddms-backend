namespace DDMS.Backend.DTOs.Boats;

public class BoatMaintenanceDto
{
    public Guid Id { get; set; }
    public Guid BoatId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBoatMaintenanceDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
}
