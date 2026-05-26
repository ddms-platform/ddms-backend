namespace DDMS.Backend.DTOs.Docks;

public class DockScheduleDto
{
    public Guid Id { get; set; }
    public Guid DockId { get; set; }
    public Guid BoatId { get; set; }
    public string? BoatName { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDockScheduleDto
{
    public Guid BoatId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? ScheduleId { get; set; }
}
