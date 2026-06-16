namespace DDMS.Backend.Models.DTOs.MaintenanceServices;

public class MaintenanceServiceItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? IconCode { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
}
