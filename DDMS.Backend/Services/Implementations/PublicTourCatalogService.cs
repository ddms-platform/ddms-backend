using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class PublicTourCatalogService : IPublicTourCatalogService
{
    private readonly IOwnerToursRepository _tourRepository;
    private readonly ITourImageRepository _tourImageRepository;
    private readonly IFaqRepository _faqRepository;

    public PublicTourCatalogService(
        IOwnerToursRepository tourRepository,
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
        var activeSchedules = entity.tour_schedules?
            .Where(s => s.status == DDMS.Backend.Common.Constants.TourConstants.ScheduleStatuses.Scheduled && s.start_time >= DateTime.UtcNow)
            .ToList() ?? new List<tour_schedule>();

        var allCabins = activeSchedules
            .Where(s => s.boat != null && s.boat.boat_cabins != null)
            .SelectMany(s => s.boat!.boat_cabins)
            .DistinctBy(c => c.name) // group by name to avoid duplicates if multiple identical boats
            .ToList();

        var allServices = activeSchedules
            .Where(s => s.boat != null && s.boat.boat_services != null)
            .SelectMany(s => s.boat!.boat_services)
            .DistinctBy(s => s.name)
            .ToList();

        return new TourItemResponse
        {
            id = entity.id,
            name = entity.name,
            price = entity.price,
            description = entity.description,
            durationMinutes = entity.duration_minutes,
            location = entity.location,
            mapUrl = entity.map_url,
            status = entity.status,
            cancelPolicy = entity.cancel_policy,
            cancelHours = entity.cancel_hours,
            avgRating = entity.avg_rating,
            totalReviews = entity.total_reviews,
            routes = entity.routes?.OrderBy(r => r.sort_order).Select(r => new TourRouteResponse
            {
                id = r.id,
                name = r.name ?? "",
                startPoint = r.start_point,
                endPoint = r.end_point,
                description = r.description
            }).ToList() ?? new List<TourRouteResponse>(),
            faqs = entity.faqs?.OrderBy(f => f.sort_order).Select(f => new TourFaqResponse
            {
                id = f.id,
                question = f.question,
                answer = f.answer
            }).ToList() ?? new List<TourFaqResponse>(),
            classes = allCabins.Select(c => new TourClassResponse
            {
                id = c.id,
                name = c.name,
                capacity = c.capacity,
                price = c.price,
                description = c.description,
                imageUrl = c.image_url
            }).ToList(),
            services = allServices.Select(s => new TourServiceResponse
            {
                id = s.id,
                name = s.name,
                price = s.price,
                description = s.description,
                imageUrl = s.image_url
            }).ToList(),
            createdBy = entity.created_by,
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
