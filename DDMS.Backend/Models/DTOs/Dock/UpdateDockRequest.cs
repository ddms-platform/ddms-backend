namespace DDMS.Backend.Models.DTOs.Dock;

public class UpdateDockRequest
{
    public string name { get; set; } = null!;
    public string? location { get; set; }
    public int maxBoats { get; set; }
}
