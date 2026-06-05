using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class PublicTourCatalogService : IPublicTourCatalogService
{
    private readonly ITourRepository _tourRepository;
    private readonly ITourImageRepository _tourImageRepository;
    private readonly IFaqRepository _faqRepository;

    public PublicTourCatalogService(
        ITourRepository tourRepository,
        ITourImageRepository tourImageRepository,
        IFaqRepository faqRepository)
    {
        _tourRepository = tourRepository;
        _tourImageRepository = tourImageRepository;
        _faqRepository = faqRepository;
    }

    public async Task<TourItemResponse> GetActiveTourAsync(Guid tourId)
    {
        var entity = await _tourRepository.GetActiveByIdAsync(tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapTour(entity);
    }

    public async Task<List<TourImageItemResponse>> GetTourImagesAsync(Guid tourId)
    {
        await EnsureActiveTourExistsAsync(tourId);
        var items = await _tourImageRepository.GetByTourIdAsync(tourId);
        return items.Select(MapImage).ToList();
    }

    public async Task<List<FaqItemResponse>> GetTourFaqsAsync(Guid tourId)
    {
        await EnsureActiveTourExistsAsync(tourId);
        var items = await _faqRepository.GetByTourIdAsync(tourId);
        return items.Select(MapFaq).ToList();
    }

    private async Task EnsureActiveTourExistsAsync(Guid tourId)
    {
        var entity = await _tourRepository.GetActiveByIdAsync(tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }
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

    private static FaqItemResponse MapFaq(faq entity)
    {
        return new FaqItemResponse
        {
            id = entity.id,
            tourId = entity.tour_id,
            question = entity.question,
            answer = entity.answer,
            sortOrder = entity.sort_order,
            createdAt = entity.created_at
        };
    }
}
