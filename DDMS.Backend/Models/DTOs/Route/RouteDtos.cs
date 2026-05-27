namespace DDMS.Backend.Models.DTOs.Route;

public class CreateRouteRequest
{
    public Guid tour_id { get; set; }
    public string? name { get; set; }
    public string start_point { get; set; } = string.Empty;
    public string end_point { get; set; } = string.Empty;
    public string? description { get; set; }
    public int sort_order { get; set; }
}

public class UpdateRouteRequest : CreateRouteRequest
{
}

public class RouteResponse
{
    public Guid id { get; set; }
    public Guid tour_id { get; set; }
    public string? name { get; set; }
    public string start_point { get; set; } = string.Empty;
    public string end_point { get; set; } = string.Empty;
    public string? description { get; set; }
    public int sort_order { get; set; }
}
