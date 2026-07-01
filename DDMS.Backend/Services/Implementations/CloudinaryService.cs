using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryService(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value;
        var account = new Account(_options.cloudName, _options.apiKey, _options.apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(Stream stream, string fileName)
    {
        EnsureConfigured();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = _options.folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
        {
            throw new AppException(
                ErrorCode.TourImageUploadFailed,
                result.Error?.Message ?? ErrorCode.Messages.TourImageUploadFailed);
        }

        return new CloudinaryUploadResult(result.SecureUrl.ToString()!, result.PublicId);
    }

    public async Task DeleteImageAsync(string publicId)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }

    public async Task<CloudinaryUploadResult> UploadVideoAsync(Stream stream, string fileName)
    {
        EnsureConfigured();

        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = _options.folder + "/videos",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
        {
            throw new AppException(
                ErrorCode.TourImageUploadFailed,
                result.Error?.Message ?? "Video upload failed");
        }

        return new CloudinaryUploadResult(result.SecureUrl.ToString()!, result.PublicId);
    }

    public async Task DeleteVideoAsync(string publicId)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Video };
        await _cloudinary.DestroyAsync(deletionParams);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.cloudName) ||
            string.IsNullOrWhiteSpace(_options.apiKey) ||
            string.IsNullOrWhiteSpace(_options.apiSecret))
        {
            throw new AppException(ErrorCode.TourImageUploadFailed, ErrorCode.Messages.TourImageUploadFailed);
        }
    }
}
