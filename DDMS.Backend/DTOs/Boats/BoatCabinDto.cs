namespace DDMS.Backend.DTOs.Boats;

public class BoatCabinDto
{
    public Guid Id { get; set; }
    public Guid BoatId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public int TotalRooms { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateBoatCabinDto
{
    public string Name { get; set; } = null!;
    public int Capacity { get; set; } = 2;
    public decimal Price { get; set; }
    public int TotalRooms { get; set; } = 1;
    public string? Description { get; set; }
}
