namespace DDMS.Backend.Models.DTOs.Tours;

public class TourListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? status { get; set; }
}
