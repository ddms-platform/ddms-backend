using DDMS.Backend.DTOs.Boats;

namespace DDMS.Backend.DTOs.Boats;

public class BoatDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public int MaxPassengers { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relations
    public List<BoatCabinDto> Cabins { get; set; } = [];
    public List<BoatServiceDto> Services { get; set; } = [];
    public List<BoatImageDto> Images { get; set; } = [];
    public List<BoatMaintenanceDto> Maintenances { get; set; } = [];

    // Aggregated
    public int TotalCabins { get; set; }
    public int TotalServices { get; set; }
    public int ActiveServices { get; set; }
}

public class CreateBoatDto
{
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public int MaxPassengers { get; set; }
    public string Status { get; set; } = "idle";
}
