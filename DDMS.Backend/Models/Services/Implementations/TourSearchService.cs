using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class TourSearchService : ITourSearchService
{
    private readonly ITourSearchRepository _tourSearchRepository;

    public TourSearchService(ITourSearchRepository tourSearchRepository)
    {
        _tourSearchRepository = tourSearchRepository;
    }

    public async Task<PagedResponse<TourSearchItemResponse>> SearchAsync(TourSearchQuery query)
    {
        ValidateQuery(query);

        var (items, total) = await _tourSearchRepository.SearchAsync(query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        var scheduleIds = items
            .SelectMany(x => x.tour_schedules)
            .Select(x => x.id)
            .Distinct()
            .ToList();
        var bookedBySchedule = await _tourSearchRepository.GetBookedCapacityByScheduleIdsAsync(scheduleIds);

        return new PagedResponse<TourSearchItemResponse>
        {
            items = items.Select(t => MapTour(t, query, bookedBySchedule)).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    private static void ValidateQuery(TourSearchQuery query)
    {
        var errors = new Dictionary<string, List<string>>();

        var sortBy = query.sortBy.Trim().ToLowerInvariant();
        if (!TourConstants.SortFields.Allowed.Contains(sortBy))
        {
            errors["sortBy"] = [ErrorCode.Messages.TourSearchSortInvalid];
        }

        var sortOrder = query.sortOrder.Trim().ToLowerInvariant();
        if (!TourConstants.SortOrders.Allowed.Contains(sortOrder))
        {
            errors["sortOrder"] = [ErrorCode.Messages.TourSearchSortOrderInvalid];
        }

        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            if (!TourConstants.Statuses.Allowed.Contains(normalizedStatus))
            {
                errors["status"] = [ErrorCode.Messages.TourStatusInvalid];
            }
        }

        if (query.minPrice.HasValue && query.maxPrice.HasValue && query.minPrice > query.maxPrice)
        {
            errors["priceRange"] = [ErrorCode.Messages.TourSearchPriceRangeInvalid];
        }

        if (query.minDurationMinutes.HasValue && query.maxDurationMinutes.HasValue &&
            query.minDurationMinutes > query.maxDurationMinutes)
        {
            errors["durationRange"] = [ErrorCode.Messages.TourSearchDurationRangeInvalid];
        }

        if (errors.Count > 0)
        {
            throw new AppException(ErrorCode.TourSearchValidationFailed, ErrorCode.Messages.ValidationFailed, errors);
        }
    }

    private static TourSearchItemResponse MapTour(
        tour entity,
        TourSearchQuery query,
        Dictionary<Guid, int> bookedBySchedule)
    {
        return new TourSearchItemResponse
        {
            id = entity.id,
            name = entity.name,
            price = entity.price,
            description = entity.description,
            durationMinutes = entity.duration_minutes,
            location = entity.location,
            status = entity.status,
            avgRating = entity.avg_rating,
            totalReviews = entity.total_reviews,
            cancelPolicy = entity.cancel_policy,
            cancelHours = entity.cancel_hours,
            availableSlots = BuildAvailableSlots(entity, query, bookedBySchedule)
        };
    }

    private static List<AvailableSlotResponse> BuildAvailableSlots(
        tour entity,
        TourSearchQuery query,
        Dictionary<Guid, int> bookedBySchedule)
    {
        var now = DateTime.UtcNow;
        var schedules = entity.tour_schedules
            .Where(s => s.status == TourConstants.ScheduleStatuses.Scheduled && s.start_time >= now);

        if (query.date.HasValue)
        {
            var dayStart = DateTime.SpecifyKind(query.date.Value.Date, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);
            schedules = schedules.Where(s => s.start_time >= dayStart && s.start_time < dayEnd);
        }

        return schedules
            .OrderBy(s => s.start_time)
            .Select(s => MapSlot(s, bookedBySchedule))
            .Where(s => s.remainingCapacity is > 0)
            .ToList();
    }

    private static AvailableSlotResponse MapSlot(tour_schedule schedule, Dictionary<Guid, int> bookedBySchedule)
    {
        var booked = bookedBySchedule.GetValueOrDefault(schedule.id, 0);
        int? maxCapacity = schedule.boat?.max_passengers;
        int? remaining = maxCapacity.HasValue ? Math.Max(0, maxCapacity.Value - booked) : null;

        return new AvailableSlotResponse
        {
            scheduleId = schedule.id,
            startTime = schedule.start_time,
            endTime = schedule.end_time,
            maxCapacity = maxCapacity,
            bookedCapacity = booked,
            remainingCapacity = remaining,
            boatId = schedule.boat_id,
            boatName = schedule.boat?.name,
            dockId = schedule.dock_id,
            dockName = schedule.dock?.name
        };
    }
}
