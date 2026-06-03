namespace DDMS.Backend.Models.DTOs.Tours;

public class CreateDockScheduleRequest
{
    public Guid dockId { get; set; }
    public Guid boatId { get; set; }
    public Guid? scheduleId { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
}
