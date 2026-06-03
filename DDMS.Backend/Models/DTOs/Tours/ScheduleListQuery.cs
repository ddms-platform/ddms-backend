namespace DDMS.Backend.Models.DTOs.Tours;

public class ScheduleListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public Guid? tourId { get; set; }
    public string? status { get; set; }
}
