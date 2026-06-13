namespace DDMS.Backend.Models.DTOs.Tours;

public class PopularDestinationResponse
{
    public string name { get; set; } = string.Empty;
    public int tours { get; set; }
    public string? imageUrl { get; set; }
}
