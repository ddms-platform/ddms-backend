namespace DDMS.Backend.Models.DTOs.Tours;

public class TourSearchQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? location { get; set; }
    public string? keyword { get; set; }
    public string? category { get; set; }
    public decimal? minPrice { get; set; }
    public decimal? maxPrice { get; set; }
    public DateTime? date { get; set; }
    public string? status { get; set; }
    public int? minDurationMinutes { get; set; }
    public int? maxDurationMinutes { get; set; }
    public string sortBy { get; set; } = "rating";
    public string sortOrder { get; set; } = "desc";
}
