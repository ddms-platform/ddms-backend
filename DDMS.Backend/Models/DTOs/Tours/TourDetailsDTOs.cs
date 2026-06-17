namespace DDMS.Backend.Models.DTOs.Tours;

public class TourRouteResponse
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string startPoint { get; set; } = string.Empty;
    public string endPoint { get; set; } = string.Empty;
    public string? description { get; set; }
}

public class TourFaqResponse
{
    public Guid id { get; set; }
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
}

public class TourClassResponse
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public int capacity { get; set; }
    public decimal price { get; set; }
    public string? description { get; set; }
    public string? imageUrl { get; set; }
}

public class TourServiceResponse
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public string? imageUrl { get; set; }
}
