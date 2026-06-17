using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourImageService : ITourImageService
{
    private readonly ITourImageRepository _tourImageRepository;
    private readonly IOwnerToursRepository _tourRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public TourImageService(
        ITourImageRepository tourImageRepository,
        IOwnerToursRepository tourRepository,
        ICloudinaryService cloudinaryService)
    {
        _tourImageRepository = tourImageRepository;
        _tourRepository = tourRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<List<TourImageItemResponse>> GetByTourIdAsync(Guid tourId, Guid userId)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        var items = await _tourImageRepository.GetByTourIdAsync(tourId);
        return items.Select(MapImage).ToList();
    }

    public async Task<TourImageItemResponse> UploadAsync(Guid tourId, Guid userId, UploadTourImageRequest request)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        ValidateUploadRequest(request);

        await using var stream = request.file.OpenReadStream();
        var uploadResult = await _cloudinaryService.UploadImageAsync(stream, request.file.FileName);

        var entity = new tour_image
        {
            id = Guid.NewGuid(),
            tour_id = tourId,
            image_url = uploadResult.ImageUrl,
            public_id = uploadResult.PublicId,
            caption = NormalizeOptional(request.caption),
            sort_order = request.sortOrder,
            created_at = DateTime.UtcNow
        };

        await _tourImageRepository.AddAsync(entity);
        return MapImage(entity);
    }

    public async Task<TourImageItemResponse> UpdateAsync(
        Guid tourId,
        Guid imageId,
        Guid userId,
        UpdateTourImageRequest request)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        ValidateSortOrder(request.sortOrder);

        var entity = await _tourImageRepository.GetByIdAsync(imageId, tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.caption = NormalizeOptional(request.caption);
        entity.sort_order = request.sortOrder;
        await _tourImageRepository.UpdateAsync(entity);
        return MapImage(entity);
    }

    public async Task DeleteAsync(Guid tourId, Guid imageId, Guid userId)
    {
        await EnsureTourOwnedAsync(tourId, userId);

        var entity = await _tourImageRepository.GetByIdAsync(imageId, tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        if (!string.IsNullOrWhiteSpace(entity.public_id))
        {
            await _cloudinaryService.DeleteImageAsync(entity.public_id);
        }

        await _tourImageRepository.DeleteAsync(entity);
    }

    public async Task<TourItemResponse> UpdateContentAsync(Guid tourId, Guid userId, UpdateTourContentRequest request)
    {
        var tour = await EnsureTourOwnedAsync(tourId, userId);
        tour.description = NormalizeOptional(request.description);
        await _tourRepository.UpdateAsync(tour);
        return MapTour(tour);
    }

    private async Task<tour> EnsureTourOwnedAsync(Guid tourId, Guid userId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId, userId);
        if (tour is null)
        {
            throw new AppException(ErrorCode.TourImageTourNotFound, ErrorCode.Messages.TourImageTourNotFound);
        }

        return tour;
    }

    private static void ValidateUploadRequest(UploadTourImageRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (request.file is null || request.file.Length == 0)
        {
            errors["file"] = [ErrorCode.Messages.TourImageFileRequired];
        }

        if (request.sortOrder < 0)
        {
            errors["sortOrder"] = [ErrorCode.Messages.TourImageSortOrderInvalid];
        }

        if (errors.Count > 0)
        {
            throw new AppException(ErrorCode.TourImageValidationFailed, ErrorCode.Messages.TourValidationFailed, errors);
        }
    }

    private static void ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new AppException(ErrorCode.TourImageValidationFailed, ErrorCode.Messages.TourValidationFailed,
                new Dictionary<string, List<string>>
                {
                    ["sortOrder"] = [ErrorCode.Messages.TourImageSortOrderInvalid]
                });
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static TourImageItemResponse MapImage(tour_image entity)
    {
        return new TourImageItemResponse
        {
            id = entity.id,
            tourId = entity.tour_id,
            imageUrl = entity.image_url,
            publicId = entity.public_id,
            caption = entity.caption,
            sortOrder = entity.sort_order,
            createdAt = entity.created_at
        };
    }

    private static TourItemResponse MapTour(tour entity)
    {
        return new TourItemResponse
        {
            id = entity.id,
            name = entity.name,
            price = entity.price,
            description = entity.description,
            durationMinutes = entity.duration_minutes,
            location = entity.location,
            status = entity.status,
            cancelPolicy = entity.cancel_policy,
            cancelHours = entity.cancel_hours,
            avgRating = entity.avg_rating,
            totalReviews = entity.total_reviews,
            createdAt = entity.created_at,
            updatedAt = entity.updated_at
        };
    }
}
