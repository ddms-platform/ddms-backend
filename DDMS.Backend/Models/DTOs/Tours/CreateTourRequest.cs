namespace DDMS.Backend.Models.DTOs.Tours;

public class CreateTourRequest
{
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public int durationMinutes { get; set; }
    public string? location { get; set; }
    public string cancelPolicy { get; set; } = "free";
    public int? cancelHours { get; set; }
}
