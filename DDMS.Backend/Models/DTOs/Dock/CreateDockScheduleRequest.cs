namespace DDMS.Backend.Models.DTOs.Dock;

public class CreateDockScheduleRequest
{
    public Guid boatId { get; set; }
    public Guid? scheduleId { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
}
