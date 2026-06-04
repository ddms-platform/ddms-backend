namespace DDMS.Backend.Models.DTOs.Boat;

public class UploadBoatImageRequest
{
    /// <summary>
    /// Base64 encoded image string. Format: "data:image/jpeg;base64,..." or raw base64.
    /// </summary>
    public string fileBase64 { get; set; } = null!;

    public string? caption { get; set; }
}
