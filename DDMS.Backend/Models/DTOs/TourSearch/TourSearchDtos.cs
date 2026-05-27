namespace DDMS.Backend.Models.DTOs.TourSearch;

public class TourSearchRequest
{
    public string? location { get; set; }
    public decimal? min_price { get; set; }
    public decimal? max_price { get; set; }
    public DateTime? date { get; set; }
    public string? status { get; set; }
    public int? min_duration_minutes { get; set; }
    public int? max_duration_minutes { get; set; }
    public string? sort_by { get; set; }
    public bool sort_desc { get; set; }
}

public class TourSearchResponse
{
    public Guid tour_id { get; set; }
    public string tour_name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public decimal avg_rating { get; set; }
    public string? location { get; set; }
    public int duration_minutes { get; set; }
    public Guid schedule_id { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public int? max_passengers { get; set; }
    public int booked_people { get; set; }
    public int? remaining_capacity { get; set; }
}
