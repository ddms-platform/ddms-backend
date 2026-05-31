namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatStatsResponse
{
    public int total { get; init; }
    public int active { get; init; }
    public int maintenance { get; init; }
    public int idle { get; init; }
}
