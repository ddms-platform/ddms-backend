namespace DDMS.Backend.Models.DTOs.Tours;

public class UploadTourImageRequest
{
    public IFormFile file { get; set; } = null!;
    public string? caption { get; set; }
    public int sortOrder { get; set; }
}
