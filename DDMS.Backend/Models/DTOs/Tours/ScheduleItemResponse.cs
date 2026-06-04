namespace DDMS.Backend.Models.DTOs.Tours;

public class ScheduleItemResponse
{
    public Guid id { get; set; }
    public Guid tourId { get; set; }
    public string tourName { get; set; } = string.Empty;
    public Guid? boatId { get; set; }
    public string? boatName { get; set; }
    public Guid? dockId { get; set; }
    public string? dockName { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public string status { get; set; } = string.Empty;
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}
