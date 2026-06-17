namespace DDMS.Backend.Models.DTOs.AdminMaintenances;

public class MaintenanceItem
{
    public Guid Id { get; set; }
    public Guid BoatId { get; set; }
    public string BoatName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? PortMaintenanceServiceId { get; set; }
    public string? PortMaintenanceServiceName { get; set; }
    public decimal? Price { get; set; }
    public string Status { get; set; } = null!;
}
