namespace DDMS.Backend.Models.DTOs.Tours;

public class AvailableSlotResponse
{
    public Guid scheduleId { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public int? maxCapacity { get; set; }
    public int bookedCapacity { get; set; }
    public int? remainingCapacity { get; set; }
    public Guid? boatId { get; set; }
    public string? boatName { get; set; }
    public Guid? dockId { get; set; }
    public string? dockName { get; set; }
}
