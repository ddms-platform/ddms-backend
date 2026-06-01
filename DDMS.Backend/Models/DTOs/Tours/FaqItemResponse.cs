namespace DDMS.Backend.Models.DTOs.Tours;

public class FaqItemResponse
{
    public Guid id { get; set; }
    public Guid tourId { get; set; }
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
    public int sortOrder { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}
