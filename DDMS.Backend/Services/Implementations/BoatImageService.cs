using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class BoatImageService : IBoatImageService
{
    private readonly IBoatImageRepository _imageRepository;
    private readonly IBoatRepository _boatRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public BoatImageService(
        IBoatImageRepository imageRepository,
        IBoatRepository boatRepository,
        ICloudinaryService cloudinaryService)
    {
        _imageRepository = imageRepository;
        _boatRepository = boatRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<List<BoatImageResponse>> GetByBoatIdAsync(Guid boatId)
    {
        await EnsureBoatExistsAsync(boatId);
        var images = await _imageRepository.GetByBoatIdAsync(boatId);
        return images.Select(MapToResponse).ToList();
    }

    public async Task<BoatImageResponse> UploadAsync(Guid boatId, string fileBase64, string? caption)
    {
        await EnsureBoatExistsAsync(boatId);

        if (string.IsNullOrWhiteSpace(fileBase64))
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["file"] = ["File ảnh là bắt buộc"]
            });
        }

        // Get current max sort order
        var existingImages = await _imageRepository.GetByBoatIdAsync(boatId);
        var nextSortOrder = existingImages.Count > 0
            ? existingImages.Max(i => i.sort_order) + 1
            : 0;

        // Strip data URL prefix nếu có và convert sang stream
        var base64Data = fileBase64.Contains(",") ? fileBase64.Split(',')[1] : fileBase64;
        var bytes = Convert.FromBase64String(base64Data);
        using var stream = new MemoryStream(bytes);

        // Upload to Cloudinary
        var uploadResult = await _cloudinaryService.UploadImageAsync(stream, $"boat_{boatId}_{Guid.NewGuid():N}.jpg");
        var imageUrl = uploadResult.ImageUrl;
        var publicId = uploadResult.PublicId;

        var image = new boat_image
        {
            id = Guid.NewGuid(),
            boat_id = boatId,
            image_url = imageUrl,
            public_id = publicId,
            caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
            sort_order = nextSortOrder,
            created_at = DateTime.UtcNow,
        };

        await _imageRepository.CreateAsync(image);
        return MapToResponse(image);
    }

    public async Task DeleteAsync(Guid boatId, Guid imageId)
    {
        await EnsureBoatExistsAsync(boatId);

        var image = await _imageRepository.GetByIdAsync(imageId)
            ?? throw new NotFoundException("Ảnh không tồn tại");

        if (image.boat_id != boatId)
            throw new ForbiddenException("Ảnh không thuộc thuyền này");

        // Delete from Cloudinary if publicId exists
        if (!string.IsNullOrWhiteSpace(image.public_id))
        {
            await _cloudinaryService.DeleteImageAsync(image.public_id);
        }

        await _imageRepository.DeleteAsync(image);
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task EnsureBoatExistsAsync(Guid boatId)
    {
        var boat = await _boatRepository.GetByIdAsync(boatId);
        if (boat is null) throw new NotFoundException("Thuyền không tồn tại");
    }

    private static BoatImageResponse MapToResponse(boat_image i) => new()
    {
        id = i.id,
        boatId = i.boat_id,
        imageUrl = i.image_url,
        publicId = i.public_id,
        caption = i.caption,
        sortOrder = i.sort_order,
        createdAt = i.created_at,
    };
}
