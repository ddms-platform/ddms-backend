namespace DDMS.Backend.DTOs.Docks;

public class DockDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public int MaxBoats { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Aggregated
    public int CurrentBoats { get; set; }
    public List<DockScheduleDto> Schedules { get; set; } = [];
}

public class CreateDockDto
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public int MaxBoats { get; set; } = 1;
}
