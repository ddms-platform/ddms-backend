namespace DDMS.Backend.Models.DTOs.Dock;

public class DockListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? search { get; set; }
}
