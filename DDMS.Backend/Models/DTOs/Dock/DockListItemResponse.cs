namespace DDMS.Backend.Models.DTOs.Dock;

public class DockListItemResponse
{
    public Guid id { get; init; }
    public string name { get; init; } = null!;
    public string? location { get; init; }
    public int maxBoats { get; init; }
    public DateTime createdAt { get; init; }
    public DateTime updatedAt { get; init; }
}
