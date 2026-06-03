namespace DDMS.Backend.Models.Services.Interfaces;

public record CloudinaryUploadResult(string ImageUrl, string PublicId);

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(Stream stream, string fileName);
    Task DeleteImageAsync(string publicId);
}
