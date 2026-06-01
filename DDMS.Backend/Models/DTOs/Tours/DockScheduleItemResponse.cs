namespace DDMS.Backend.Models.DTOs.Tours;

public class DockScheduleItemResponse
{
    public Guid id { get; set; }
    public Guid dockId { get; set; }
    public string dockName { get; set; } = string.Empty;
    public Guid boatId { get; set; }
    public string boatName { get; set; } = string.Empty;
    public Guid? scheduleId { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public DateTime createdAt { get; set; }
}
