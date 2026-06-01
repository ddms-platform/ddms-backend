namespace DDMS.Backend.Models.DTOs.Tours;

public class UpdateFaqRequest
{
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
    public int sortOrder { get; set; }
}
