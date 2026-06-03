namespace DDMS.Backend.Models.DTOs.Tours;

public class RouteListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public Guid? tourId { get; set; }
}
