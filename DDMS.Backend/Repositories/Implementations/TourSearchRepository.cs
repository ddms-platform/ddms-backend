using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.TourSearch;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class TourSearchRepository : ITourSearchRepository
{
    private readonly AppDbContext _db;

    public TourSearchRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TourSearchResponse>> SearchAsync(TourSearchRequest request, CancellationToken cancellationToken)
    {
        var query = _db.tour_schedules
            .AsNoTracking()
            .Include(x => x.tour)
            .Include(x => x.boat)
            .Where(x => x.boat == null
                || (x.boat.compliance_status != BoatComplianceStatuses.Hidden
                    && x.boat.compliance_status != BoatComplianceStatuses.Locked))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.location))
        {
            query = query.Where(x => x.tour.location != null && x.tour.location.Contains(request.location));
        }

        if (!string.IsNullOrWhiteSpace(request.status))
        {
            query = query.Where(x => x.tour.status == request.status);
        }

        if (request.min_price.HasValue)
        {
            query = query.Where(x => x.tour.price >= request.min_price.Value);
        }

        if (request.max_price.HasValue)
        {
            query = query.Where(x => x.tour.price <= request.max_price.Value);
        }

        if (request.min_duration_minutes.HasValue)
        {
            query = query.Where(x => x.tour.duration_minutes >= request.min_duration_minutes.Value);
        }

        if (request.max_duration_minutes.HasValue)
        {
            query = query.Where(x => x.tour.duration_minutes <= request.max_duration_minutes.Value);
        }

        if (request.date.HasValue)
        {
            var targetDate = request.date.Value.Date;
            query = query.Where(x => x.start_time.Date == targetDate);
        }

        var schedules = await query.ToListAsync(cancellationToken);
        var scheduleIds = schedules.Select(x => x.id).ToList();
        var bookings = await _db.bookings
            .AsNoTracking()
            .Where(x => scheduleIds.Contains(x.schedule_id) && x.status != "cancelled")
            .GroupBy(x => x.schedule_id)
            .Select(x => new { schedule_id = x.Key, total_people = x.Sum(y => y.num_people) })
            .ToListAsync(cancellationToken);

        var bookingMap = bookings.ToDictionary(x => x.schedule_id, x => x.total_people);

        var result = schedules.Select(x =>
        {
            var booked = bookingMap.GetValueOrDefault(x.id, 0);
            int? remaining = x.boat == null ? null : x.boat.max_passengers - booked;

            return new TourSearchResponse
            {
                tour_id = x.tour_id,
                tour_name = x.tour.name,
                price = x.tour.price,
                avg_rating = x.tour.avg_rating,
                location = x.tour.location,
                duration_minutes = x.tour.duration_minutes,
                schedule_id = x.id,
                start_time = x.start_time,
                end_time = x.end_time,
                max_passengers = x.boat?.max_passengers,
                booked_people = booked,
                remaining_capacity = remaining
            };
        });

        result = ApplySort(result, request.sort_by, request.sort_desc);
        return result.ToList();
    }

    private static IEnumerable<TourSearchResponse> ApplySort(IEnumerable<TourSearchResponse> source, string? sortBy, bool sortDesc)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "price" => sortDesc ? source.OrderByDescending(x => x.price) : source.OrderBy(x => x.price),
            "rating" => sortDesc ? source.OrderByDescending(x => x.avg_rating) : source.OrderBy(x => x.avg_rating),
            _ => source.OrderBy(x => x.start_time)
        };
    }
}
