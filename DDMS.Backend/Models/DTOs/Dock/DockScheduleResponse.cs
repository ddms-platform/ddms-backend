namespace DDMS.Backend.Models.DTOs.Dock;

public class DockScheduleResponse
{
    public Guid id { get; init; }
    public Guid dockId { get; init; }
    public Guid boatId { get; init; }
    public string boatName { get; init; } = null!;
    public Guid? scheduleId { get; init; }
    public DateTime startTime { get; init; }
    public DateTime endTime { get; init; }
    public DateTime createdAt { get; init; }
}
