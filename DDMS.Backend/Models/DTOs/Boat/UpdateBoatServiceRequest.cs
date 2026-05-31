namespace DDMS.Backend.Models.DTOs.Boat;

public class UpdateBoatServiceRequest
{
    public string name { get; set; } = null!;
    public decimal price { get; set; }
    public string? description { get; set; }
    public bool isActive { get; set; }
}
