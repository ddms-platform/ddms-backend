namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? search { get; set; }
    public string? status { get; set; }  // idle, running
    public string? type { get; set; }   // cruise, luxury, standard, party, speedboat
}
