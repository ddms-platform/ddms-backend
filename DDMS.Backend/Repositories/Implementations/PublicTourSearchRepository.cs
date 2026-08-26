using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class PublicTourSearchRepository : IPublicTourSearchRepository
{
    private readonly AppDbContext _dbContext;

    public PublicTourSearchRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<tour> items, int total)> SearchAsync(TourSearchQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var now = DateTime.UtcNow;

        var toursQuery = _dbContext.tours
            .Include(x => x.tour_images)
            .Include(x => x.tour_schedules)
                .ThenInclude(s => s.boat)
            .Include(x => x.tour_schedules)
                .ThenInclude(s => s.dock)
            .Where(x => x.tour_schedules.Any(ts => ts.boat != null && !ts.boat.is_deleted))
            .AsQueryable();

        if (string.IsNullOrWhiteSpace(query.status))
        {
            toursQuery = toursQuery.Where(x => x.status == TourConstants.Statuses.Active);
        }
        else
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            toursQuery = toursQuery.Where(x => x.status.ToLower() == normalizedStatus);
        }

        var searchKey = !string.IsNullOrWhiteSpace(query.keyword) ? query.keyword : query.location;
        if (!string.IsNullOrWhiteSpace(searchKey))
        {
            var keyword = searchKey.Trim().ToLower();
            toursQuery = toursQuery.Where(x =>
                x.name.ToLower().Contains(keyword) ||
                (x.location != null && x.location.ToLower().Contains(keyword)) ||
                (x.description != null && x.description.ToLower().Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.category) && query.category.Trim().ToLowerInvariant() != "all")
        {
            var cat = query.category.Trim().ToLowerInvariant();
            switch (cat)
            {
                case "cruise":
                    toursQuery = toursQuery.Where(x =>
                        x.name.ToLower().Contains("thuyền") ||
                        x.name.ToLower().Contains("cruise") ||
                        x.name.ToLower().Contains("yacht") ||
                        (x.description != null && (x.description.ToLower().Contains("thuyền") || x.description.ToLower().Contains("cruise"))));
                    break;
                case "sunset":
                    toursQuery = toursQuery.Where(x =>
                        x.name.ToLower().Contains("hoàng hôn") ||
                        x.name.ToLower().Contains("sunset") ||
                        (x.description != null && (x.description.ToLower().Contains("hoàng hôn") || x.description.ToLower().Contains("sunset"))));
                    break;
                case "dinner":
                    toursQuery = toursQuery.Where(x =>
                        x.name.ToLower().Contains("ăn") ||
                        x.name.ToLower().Contains("dinner") ||
                        x.name.ToLower().Contains("ẩm thực") ||
                        x.name.ToLower().Contains("tiệc") ||
                        (x.description != null && (x.description.ToLower().Contains("ăn") || x.description.ToLower().Contains("ẩm thực") || x.description.ToLower().Contains("dinner"))));
                    break;
                case "party":
                    toursQuery = toursQuery.Where(x =>
                        x.name.ToLower().Contains("party") ||
                        x.name.ToLower().Contains("tiệc") ||
                        x.name.ToLower().Contains("sự kiện") ||
                        (x.description != null && (x.description.ToLower().Contains("party") || x.description.ToLower().Contains("tiệc"))));
                    break;
                case "family":
                    toursQuery = toursQuery.Where(x =>
                        x.name.ToLower().Contains("gia đình") ||
                        x.name.ToLower().Contains("family") ||
                        (x.description != null && (x.description.ToLower().Contains("gia đình") || x.description.ToLower().Contains("family"))));
                    break;
                case "sightseeing":
                    toursQuery = toursQuery.Where(x =>
                        x.name.ToLower().Contains("ngắm") ||
                        x.name.ToLower().Contains("tham quan") ||
                        x.name.ToLower().Contains("sightseeing") ||
                        x.name.ToLower().Contains("cầu rồng") ||
                        (x.description != null && (x.description.ToLower().Contains("ngắm") || x.description.ToLower().Contains("tham quan"))));
                    break;
            }
        }

        if (query.ownerId.HasValue)
        {
            var ownerId = query.ownerId.Value;
            toursQuery = toursQuery.Where(x =>
                x.created_by == ownerId
                || x.tour_schedules.Any(s => s.boat != null && s.boat.owner_id == ownerId));
        }

        if (query.minPrice.HasValue)
        {
            toursQuery = toursQuery.Where(x => x.price >= query.minPrice.Value);
        }

        if (query.maxPrice.HasValue)
        {
            toursQuery = toursQuery.Where(x => x.price <= query.maxPrice.Value);
        }

        if (query.minDurationMinutes.HasValue)
        {
            toursQuery = toursQuery.Where(x => x.duration_minutes >= query.minDurationMinutes.Value);
        }

        if (query.maxDurationMinutes.HasValue)
        {
            toursQuery = toursQuery.Where(x => x.duration_minutes <= query.maxDurationMinutes.Value);
        }

        if (query.date.HasValue)
        {
            var dayStart = DateTime.SpecifyKind(query.date.Value.Date, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);
            toursQuery = toursQuery.Where(x => x.tour_schedules.Any(s =>
                s.status == TourConstants.ScheduleStatuses.Scheduled &&
                s.start_time >= dayStart &&
                s.start_time < dayEnd &&
                (s.boat == null
                    || (s.boat.compliance_status != BoatComplianceStatuses.Hidden
                        && s.boat.compliance_status != BoatComplianceStatuses.Locked))));
        }
        else
        {
            toursQuery = toursQuery.Where(x => x.tour_schedules.Any(s =>
                s.status == TourConstants.ScheduleStatuses.Scheduled &&
                s.start_time >= now &&
                (s.boat == null
                    || (s.boat.compliance_status != BoatComplianceStatuses.Hidden
                        && s.boat.compliance_status != BoatComplianceStatuses.Locked))));
        }

        var sortBy = query.sortBy.Trim().ToLowerInvariant();
        var sortOrder = query.sortOrder.Trim().ToLowerInvariant();
        toursQuery = ApplySorting(toursQuery, sortBy, sortOrder);

        var total = await toursQuery.CountAsync();
        var items = await toursQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Dictionary<Guid, int>> GetBookedCapacityByScheduleIdsAsync(IEnumerable<Guid> scheduleIds)
    {
        var ids = scheduleIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.bookings
            .Where(x => ids.Contains(x.schedule_id) && x.status != "cancelled")
            .GroupBy(x => x.schedule_id)
            .Select(g => new { ScheduleId = g.Key, Booked = g.Sum(x => x.num_people) })
            .ToDictionaryAsync(x => x.ScheduleId, x => x.Booked);
    }

    public async Task<List<PopularDestinationResponse>> GetPopularDestinationsAsync(int limit)
    {
        var destinations = await _dbContext.tours
            .Where(t => t.status == TourConstants.Statuses.Active && t.location != null && t.tour_schedules.Any(ts => ts.boat != null && !ts.boat.is_deleted))
            .GroupBy(t => t.location)
            .Select(g => new PopularDestinationResponse
            {
                name = g.Key!,
                tours = g.Count(),
                imageUrl = g.SelectMany(t => t.tour_images).OrderBy(i => i.sort_order).Select(i => i.image_url).FirstOrDefault()
            })
            .OrderByDescending(d => d.tours)
            .Take(limit)
            .ToListAsync();

        return destinations;
    }

    private static IQueryable<tour> ApplySorting(IQueryable<tour> query, string sortBy, string sortOrder)
    {
        var descending = sortOrder != TourConstants.SortOrders.Asc;

        return sortBy switch
        {
            TourConstants.SortFields.Price => descending
                ? query.OrderByDescending(x => x.price)
                : query.OrderBy(x => x.price),
            TourConstants.SortFields.Rating => descending
                ? query.OrderByDescending(x => x.avg_rating).ThenByDescending(x => x.total_reviews)
                : query.OrderBy(x => x.avg_rating).ThenBy(x => x.total_reviews),
            _ => descending
                ? query.OrderByDescending(x => x.avg_rating).ThenByDescending(x => x.total_reviews)
                : query.OrderBy(x => x.avg_rating).ThenBy(x => x.total_reviews)
        };
    }
}
