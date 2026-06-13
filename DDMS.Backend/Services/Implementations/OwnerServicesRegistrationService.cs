using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class OwnerServicesRegistrationService : IOwnerServicesRegistrationService
{
    private readonly ITourService _tourService;
    private readonly IOwnerServicesRegistrationRepository _repo;
    private readonly IEmailSender _email;

    public OwnerServicesRegistrationService(
        ITourService tourService,
        IOwnerServicesRegistrationRepository repo,
        IEmailSender email)
    {
        _tourService = tourService;
        _repo = repo;
        _email = email;
    }

    public async Task<TourResponse> RegisterAsync(DynamicServiceRequest request, CancellationToken ct)
    {
        var createTourReq = new CreateTourRequest
        {
            name = request.name,
            price = request.basePrice,
            description = BuildDescription(request),
            duration_minutes = OwnerServiceRegistrationDefaults.TourDurationMinutes,
            location = OwnerServiceRegistrationDefaults.TourLocation,
            status = OwnerServiceRegistrationDefaults.TourPendingStatus,
            cancel_policy = OwnerServiceRegistrationDefaults.CancelPolicy
        };

        var tour = await _tourService.CreateAsync(createTourReq, ct);
        var now = DateTime.UtcNow;

        AddCabins(request, now);
        AddCombos(request, now);
        AddFaqs(request, tour.id, now);
        AddRoutes(request, tour.id, now);
        AddDefaultSchedule(request.boatId, tour.id, now);

        await _repo.SaveChangesAsync(ct);

        await TrySendConfirmationEmailAsync(request, ct);
        return tour;
    }

    private static string BuildDescription(DynamicServiceRequest r)
    {
        var s = r.description ?? string.Empty;
        if (!string.IsNullOrEmpty(r.route))       s += $"\n| Lộ trình: {r.route}";
        if (!string.IsNullOrEmpty(r.equipments))  s += $"\n| Dụng cụ: {r.equipments}";
        if (r.pricePerDay.HasValue)               s += $"\n| Giá nguyên ngày: {r.pricePerDay} VNĐ";
        return s;
    }

    private void AddCabins(DynamicServiceRequest req, DateTime now)
    {
        foreach (var r in req.rooms ?? Enumerable.Empty<ServiceRoom>())
        {
            _repo.AddBoatCabin(new boat_cabin
            {
                id = Guid.NewGuid(),
                boat_id = req.boatId,
                name = r.name,
                capacity = r.capacity,
                price = r.price ?? 0,
                description = r.description,
                image_url = r.imageUrl,
                total_rooms = 1,
                created_at = now,
                updated_at = now
            });
        }
    }

    private void AddCombos(DynamicServiceRequest req, DateTime now)
    {
        foreach (var c in req.combos ?? Enumerable.Empty<ServiceCombo>())
        {
            _repo.AddBoatService(new boat_service
            {
                id = Guid.NewGuid(),
                boat_id = req.boatId,
                name = c.name,
                price = c.price,
                description = c.description,
                image_url = c.imageUrl,
                is_active = true,
                created_at = now,
                updated_at = now
            });
        }
    }

    private void AddFaqs(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var sort = 1;
        foreach (var f in req.faqs ?? Enumerable.Empty<ServiceFaq>())
        {
            _repo.AddFaq(new faq
            {
                id = Guid.NewGuid(),
                tour_id = tourId,
                question = f.question,
                answer = f.answer,
                sort_order = sort++,
                created_at = now,
                updated_at = now
            });
        }
    }

    private void AddRoutes(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var sort = 1;
        foreach (var r in req.routes ?? Enumerable.Empty<ServiceRoute>())
        {
            _repo.AddRoute(new route
            {
                id = Guid.NewGuid(),
                tour_id = tourId,
                name = r.name,
                start_point = r.startPoint,
                end_point = r.endPoint,
                description = r.description,
                sort_order = sort++,
                created_at = now
            });
        }
    }

    private void AddDefaultSchedule(Guid boatId, Guid tourId, DateTime now)
    {
        var start = now.Add(OwnerServiceRegistrationDefaults.ScheduleStartOffset);
        _repo.AddTourSchedule(new tour_schedule
        {
            id = Guid.NewGuid(),
            tour_id = tourId,
            boat_id = boatId,
            start_time = start,
            end_time = start.AddMinutes(OwnerServiceRegistrationDefaults.TourDurationMinutes),
            status = TourScheduleStatuses.Scheduled,
            created_at = now,
            updated_at = now
        });
    }

    private async Task TrySendConfirmationEmailAsync(DynamicServiceRequest req, CancellationToken ct)
    {
        try
        {
            var boat = await _repo.FindBoatWithOwnerAsync(req.boatId, ct);
            if (boat?.owner == null) return;

            await _email.SendServiceRegistrationSuccessEmailAsync(
                boat.owner.email,
                boat.owner.full_name ?? OwnerServiceRegistrationDefaults.DefaultOwnerName,
                req.name,
                boat.name,
                req.basePrice);
        }
        catch
        {
            // best-effort: nuốt lỗi email, không fail request
        }
    }
}
