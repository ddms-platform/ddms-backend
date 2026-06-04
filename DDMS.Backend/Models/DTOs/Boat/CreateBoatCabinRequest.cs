namespace DDMS.Backend.Models.DTOs.Boat;

public class CreateBoatCabinRequest
{
    public string name { get; set; } = null!;
    public int capacity { get; set; }
    public decimal price { get; set; }
    public int totalRooms { get; set; }
    public string? description { get; set; }
}
