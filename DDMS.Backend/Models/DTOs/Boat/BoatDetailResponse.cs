namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatDetailResponse
{
    public Guid id { get; init; }
    public string name { get; init; } = null!;
    public string? type { get; init; }
    public int maxPassengers { get; init; }
    public string status { get; init; } = null!;
    public DateTime createdAt { get; init; }
    public DateTime updatedAt { get; init; }
    public List<BoatCabinResponse> cabins { get; init; } = [];
    public List<BoatServiceResponse> services { get; init; } = [];
    public List<BoatImageResponse> images { get; init; } = [];
}
