namespace DDMS.Backend.Models.DTOs.Tours;

public class CreateScheduleRequest
{
    public Guid tourId { get; set; }
    public Guid? boatId { get; set; }
    public Guid? dockId { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
}
