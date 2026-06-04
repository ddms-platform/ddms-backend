namespace DDMS.Backend.Models.DTOs.Boat;

public class OwnerBoatListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? search { get; set; }
    public string? status { get; set; }
    public string? type { get; set; }
}
