namespace DDMS.Backend.Models.DTOs.Tours;

public class DockScheduleListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public Guid? dockId { get; set; }
    public Guid? boatId { get; set; }
}
