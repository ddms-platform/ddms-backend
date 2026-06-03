namespace DDMS.Backend.Models.DTOs.Boat;

public class CreateBoatRequest
{
    public string name { get; set; } = null!;
    public string? type { get; set; }
    public int maxPassengers { get; set; }
    public string status { get; set; } = "idle";
}
