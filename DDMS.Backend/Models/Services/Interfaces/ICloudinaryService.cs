namespace DDMS.Backend.Models.Services.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload ảnh từ base64 string hoặc URL lên Cloudinary.
    /// </summary>
    /// <param name="fileBase64">Chuỗi base64 của file ảnh (data:image/...;base64,...)</param>
    /// <param name="folder">Thư mục trên Cloudinary</param>
    /// <returns>(imageUrl, publicId)</returns>
    Task<(string imageUrl, string publicId)> UploadAsync(string fileBase64, string? folder = null);

    /// <summary>
    /// Xóa ảnh khỏi Cloudinary theo publicId.
    /// </summary>
    Task DeleteAsync(string publicId);
}
