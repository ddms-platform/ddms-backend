namespace DDMS.Backend.Configurations;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string cloudName { get; set; } = string.Empty;
    public string apiKey { get; set; } = string.Empty;
    public string apiSecret { get; set; } = string.Empty;
    public string folder { get; set; } = "ddms/tours";
}
