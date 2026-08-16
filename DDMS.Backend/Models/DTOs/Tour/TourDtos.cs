namespace DDMS.Backend.Models.DTOs.Tour;

public class CreateTourRequest
{
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public int duration_minutes { get; set; }
    public string? location { get; set; }
    public string status { get; set; } = "active";
    public string cancel_policy { get; set; } = "free";
    public int? cancel_hours { get; set; }
    public Guid? created_by { get; set; }
}

public class UpdateTourRequest : CreateTourRequest
{
}

public class TourFilterRequest
{
    public string? status { get; set; }
    public string? location { get; set; }
}

public class TourResponse
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public int duration_minutes { get; set; }
    public string? location { get; set; }
    public decimal avg_rating { get; set; }
    public int total_reviews { get; set; }
    public string status { get; set; } = string.Empty;
    public string cancel_policy { get; set; } = string.Empty;
    public int? cancel_hours { get; set; }
}
