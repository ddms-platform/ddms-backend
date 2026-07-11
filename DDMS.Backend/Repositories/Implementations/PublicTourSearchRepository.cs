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

        if (!string.IsNullOrWhiteSpace(query.location))
        {
            var keyword = query.location.Trim().ToLowerInvariant();
            toursQuery = toursQuery.Where(x =>
                x.location != null && x.location.ToLower().Contains(keyword));
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
            .Where(t => t.status == TourConstants.Statuses.Active && t.location != null)
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
