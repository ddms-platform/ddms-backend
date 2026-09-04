using System.Text.Json;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class OwnerServicesRegistrationService : IOwnerServicesRegistrationService
{
    private static readonly JsonSerializerOptions PayloadJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly ITourService _tourService;
    private readonly IOwnerServicesRegistrationRepository _repo;
    private readonly IBoatRepository _boatRepo;
    private readonly IOwnerDocumentService _docService;
    private readonly IEmailSender _email;

    public OwnerServicesRegistrationService(
        ITourService tourService,
        IOwnerServicesRegistrationRepository repo,
        IBoatRepository boatRepo,
        IOwnerDocumentService docService,
        IEmailSender email)
    {
        _tourService = tourService;
        _repo = repo;
        _boatRepo = boatRepo;
        _docService = docService;
        _email = email;
    }

    public async Task<TourResponse> RegisterAsync(
        DynamicServiceRequest request, Guid userId, CancellationToken ct)
    {
        var boat = request.boatId != Guid.Empty
            ? await _boatRepo.GetByIdAsync(request.boatId)
            : null;

        if (boat == null || boat.owner_id != userId)
        {
            throw new AppException(
                ErrorCode.Forbidden,
                "Thuyền không tồn tại hoặc không thuộc quyền quản lý của bạn.");
        }

        var ownerId = boat.owner_id;

        if (request.maxGuests is <= 0)
        {
            throw new AppException(
                ErrorCode.TourValidationFailed,
                ErrorCode.Messages.TourValidationFailed,
                new Dictionary<string, List<string>>
                {
                    ["maxGuests"] = [ErrorCode.Messages.TourMaxGuestsInvalid]
                });
        }

        var docOverview = await _docService.GetOverviewByUserIdAsync(userId, ct);
        if (docOverview.IsLocked)
        {
            throw new AppException(
                ErrorCode.OwnerDocumentOverdueBlocked,
                "Tài khoản của bạn đang bị tạm khóa do chưa hoàn tất phê duyệt giấy tờ pháp lý. Không thể đăng ký thêm dịch vụ hoặc tour mới!");
        }

        tour? existingTour = null;
        if (request.id.HasValue && request.id.Value != Guid.Empty)
        {
            existingTour = await _repo.FindTourByIdAsync(request.id.Value, ct);
            if (existingTour == null)
            {
                throw new NotFoundException(
                    ErrorCode.TourNotFound,
                    "Không tìm thấy dịch vụ/tour để cập nhật. Không tạo tour mới.");
            }

            if (existingTour.created_by.HasValue && ownerId.HasValue
                && existingTour.created_by.Value != ownerId.Value)
            {
                throw new AppException(
                    ErrorCode.Forbidden,
                    "Dịch vụ này không thuộc quyền quản lý của bạn.");
            }
        }

        var now = DateTime.UtcNow;

        if (existingTour != null && IsReviewedTour(existingTour.status))
        {
            var change = await QueueServiceChangeAsync(
                existingTour, request, boat.id, ownerId ?? userId, now, ct);
            await _repo.SaveChangesAsync(ct);
            return MapTour(existingTour, ServiceApprovalKinds.ServiceChange, change.id);
        }

        if (existingTour != null)
        {
            await ApplyPayloadAsync(existingTour, request, now, resetToPending: true, ct);
            await _repo.SaveChangesAsync(ct);
            await TrySendConfirmationEmailAsync(request, ct);
            return MapTour(existingTour, ServiceApprovalKinds.TourResubmit, null);
        }

        var createTourReq = new CreateTourRequest
        {
            name = request.name,
            price = request.basePrice,
            child_price_percent = request.childPricePercent,
            infant_price_percent = request.infantPricePercent,
            description = BuildDescription(request),
            duration_minutes = OwnerServiceRegistrationDefaults.TourDurationMinutes,
            max_guests = request.maxGuests,
            location = OwnerServiceRegistrationDefaults.TourLocation,
            service_type = NormalizeServiceType(request.serviceType),
            status = OwnerServiceRegistrationDefaults.TourPendingStatus,
            cancel_policy = OwnerServiceRegistrationDefaults.CancelPolicy,
            created_by = ownerId
        };

        var tour = await _tourService.CreateAsync(createTourReq, ct);
        AddCabins(request, tour.id, now);
        AddCombos(request, tour.id, now);
        EnsureTourLinkedToBoat(request, tour.id, now);
        AddFaqs(request, tour.id, now);
        AddRoutes(request, tour.id, now);
        AddTourImages(request, tour.id, now);

        await _repo.SaveChangesAsync(ct);
        await TrySendConfirmationEmailAsync(request, ct);
        tour.approvalKind = ServiceApprovalKinds.NewTour;
        return tour;
    }

    public async Task<List<ServiceChangeRequestResponse>> ListChangesAsync(
        string? status, CancellationToken ct)
    {
        var items = await _repo.ListChangesAsync(status, ct);
        return items.Select(MapChange).ToList();
    }

    public async Task<ServiceChangeRequestResponse> ApproveChangeAsync(
        Guid changeId, CancellationToken ct)
    {
        var change = await _repo.FindChangeByIdAsync(changeId, ct)
            ?? throw new NotFoundException(
                ErrorCode.ServiceChangeNotFound,
                ErrorCode.Messages.ServiceChangeNotFound);

        if (!string.Equals(change.status, ServiceChangeStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(
                ErrorCode.ServiceChangeAlreadyProcessed,
                ErrorCode.Messages.ServiceChangeAlreadyProcessed);
        }

        var tour = change.tour ?? await _repo.FindTourByIdAsync(change.tour_id, ct)
            ?? throw new NotFoundException(ErrorCode.TourNotFound, ErrorCode.Messages.TourNotFound);

        var payload = DeserializePayload(change.payload_json);
        payload.boatId = change.boat_id;
        payload.id = tour.id;

        await ApplyPayloadAsync(tour, payload, DateTime.UtcNow, resetToPending: false, ct);
        change.status = ServiceChangeStatuses.Approved;
        change.rejection_reason = null;
        change.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);

        return MapChange(change);
    }

    public async Task<ServiceChangeRequestResponse> RejectChangeAsync(
        Guid changeId, string reason, CancellationToken ct)
    {
        var trimmed = reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new AppException(
                ErrorCode.TourValidationFailed,
                ErrorCode.Messages.ServiceChangeRejectReasonRequired);
        }

        var change = await _repo.FindChangeByIdAsync(changeId, ct)
            ?? throw new NotFoundException(
                ErrorCode.ServiceChangeNotFound,
                ErrorCode.Messages.ServiceChangeNotFound);

        if (!string.Equals(change.status, ServiceChangeStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(
                ErrorCode.ServiceChangeAlreadyProcessed,
                ErrorCode.Messages.ServiceChangeAlreadyProcessed);
        }

        change.status = ServiceChangeStatuses.Rejected;
        change.rejection_reason = trimmed;
        change.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);

        return MapChange(change);
    }

    private async Task<service_change_request> QueueServiceChangeAsync(
        tour existingTour,
        DynamicServiceRequest request,
        Guid boatId,
        Guid ownerId,
        DateTime now,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(request, PayloadJson);
        var pending = await _repo.FindPendingChangeByTourIdAsync(existingTour.id, ct);
        if (pending != null)
        {
            pending.payload_json = json;
            pending.boat_id = boatId;
            pending.owner_id = ownerId;
            pending.updated_at = now;
            pending.rejection_reason = null;
            return pending;
        }

        var created = new service_change_request
        {
            id = Guid.NewGuid(),
            tour_id = existingTour.id,
            boat_id = boatId,
            owner_id = ownerId,
            payload_json = json,
            status = ServiceChangeStatuses.Pending,
            created_at = now,
            updated_at = now,
        };
        _repo.AddChangeRequest(created);
        return created;
    }

    private async Task ApplyPayloadAsync(
        tour existingTour,
        DynamicServiceRequest request,
        DateTime now,
        bool resetToPending,
        CancellationToken ct)
    {
        existingTour.name = request.name.Trim();
        existingTour.price = request.basePrice;
        if (request.childPricePercent is not null)
            existingTour.child_price_percent = request.childPricePercent.Value;
        if (request.infantPricePercent is not null)
            existingTour.infant_price_percent = request.infantPricePercent.Value;
        if (request.maxGuests is not null)
            existingTour.max_guests = request.maxGuests.Value;
        existingTour.description = BuildDescription(request);
        existingTour.service_type = NormalizeServiceType(request.serviceType);
        existingTour.updated_at = now;

        if (resetToPending)
        {
            existingTour.status = OwnerServiceRegistrationDefaults.TourPendingStatus;
            existingTour.rejection_reason = null;
        }

        await _repo.RemoveFaqsByTourIdAsync(existingTour.id, ct);
        AddFaqs(request, existingTour.id, now);

        await _repo.RemoveRoutesByTourIdAsync(existingTour.id, ct);
        AddRoutes(request, existingTour.id, now);

        if (request.imageUrls != null)
        {
            await _repo.RemoveImagesByTourIdAsync(existingTour.id, ct);
            AddTourImages(request, existingTour.id, now);
        }

        if (request.rooms != null)
        {
            await _repo.RemoveCabinsByTourIdAsync(existingTour.id, ct);
            AddCabins(request, existingTour.id, now);
        }

        if (request.combos != null)
        {
            await _repo.RemoveCombosByTourIdAsync(existingTour.id, ct);
            AddCombos(request, existingTour.id, now);
        }
    }

    private static bool IsReviewedTour(string? status)
    {
        var value = (status ?? string.Empty).Trim().ToLowerInvariant();
        return value is TourConstants.Statuses.Active or TourConstants.Statuses.Inactive;
    }

    private static string? NormalizeServiceType(string? raw)
    {
        var value = raw?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(value) || !ServiceTypes.IsValid(value) ? null : value;
    }

    private static string BuildDescription(DynamicServiceRequest r)
    {
        var s = r.description ?? string.Empty;
        if (!string.IsNullOrEmpty(r.route)) s += $"\n| Lộ trình: {r.route}";
        if (!string.IsNullOrEmpty(r.equipments)) s += $"\n| Dụng cụ: {r.equipments}";
        if (r.pricePerDay.HasValue) s += $"\n| Giá nguyên ngày: {r.pricePerDay} VNĐ";
        return s;
    }

    private void AddCabins(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        foreach (var r in req.rooms ?? Enumerable.Empty<ServiceRoom>())
        {
            if (string.IsNullOrWhiteSpace(r.name)) continue;
            _repo.AddBoatCabin(new boat_cabin
            {
                id = Guid.NewGuid(),
                boat_id = req.boatId,
                tour_id = tourId,
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

    private void AddCombos(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        foreach (var c in req.combos ?? Enumerable.Empty<ServiceCombo>())
        {
            if (string.IsNullOrWhiteSpace(c.name)) continue;
            _repo.AddBoatService(new boat_service
            {
                id = Guid.NewGuid(),
                boat_id = req.boatId,
                tour_id = tourId,
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

    private void EnsureTourLinkedToBoat(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var hasRooms = req.rooms != null && req.rooms.Any(r => !string.IsNullOrWhiteSpace(r.name));
        var hasCombos = req.combos != null && req.combos.Any(c => !string.IsNullOrWhiteSpace(c.name));
        if (hasRooms || hasCombos) return;

        _repo.AddBoatService(new boat_service
        {
            id = Guid.NewGuid(),
            boat_id = req.boatId,
            tour_id = tourId,
            name = req.name.Trim(),
            price = req.basePrice,
            description = req.description,
            is_active = true,
            created_at = now,
            updated_at = now
        });
    }

    private void AddFaqs(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var sort = 1;
        foreach (var f in req.faqs ?? Enumerable.Empty<ServiceFaq>())
        {
            if (string.IsNullOrWhiteSpace(f.question) && string.IsNullOrWhiteSpace(f.answer))
            {
                continue;
            }

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

    private void AddTourImages(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var sort = 0;
        foreach (var rawUrl in req.imageUrls ?? Enumerable.Empty<string>())
        {
            var url = rawUrl?.Trim();
            if (string.IsNullOrWhiteSpace(url)) continue;

            _repo.AddTourImage(new tour_image
            {
                id = Guid.NewGuid(),
                tour_id = tourId,
                image_url = url,
                sort_order = sort++,
                created_at = now
            });
        }
    }

    private void AddRoutes(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var sort = 1;
        foreach (var r in req.routes ?? Enumerable.Empty<ServiceRoute>())
        {
            if (string.IsNullOrWhiteSpace(r.name)
                && string.IsNullOrWhiteSpace(r.startPoint)
                && string.IsNullOrWhiteSpace(r.endPoint))
            {
                continue;
            }

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
            // best-effort
        }
    }

    private static TourResponse MapTour(tour source, string approvalKind, Guid? changeRequestId)
    {
        return new TourResponse
        {
            id = source.id,
            name = source.name,
            description = source.description,
            price = source.price,
            duration_minutes = source.duration_minutes,
            max_guests = source.max_guests,
            location = source.location,
            avg_rating = source.avg_rating,
            total_reviews = source.total_reviews,
            status = source.status,
            cancel_policy = source.cancel_policy,
            cancel_hours = source.cancel_hours,
            approvalKind = approvalKind,
            changeRequestId = changeRequestId,
        };
    }

    private static ServiceChangeRequestResponse MapChange(service_change_request source)
    {
        return new ServiceChangeRequestResponse
        {
            id = source.id,
            tourId = source.tour_id,
            tourName = source.tour?.name ?? string.Empty,
            tourStatus = source.tour?.status,
            currentPrice = source.tour?.price ?? 0,
            boatId = source.boat_id,
            boatName = source.boat?.name,
            ownerId = source.owner_id,
            status = source.status,
            rejectionReason = source.rejection_reason,
            createdAt = source.created_at,
            updatedAt = source.updated_at,
            proposed = DeserializePayload(source.payload_json),
        };
    }

    private static DynamicServiceRequest DeserializePayload(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new DynamicServiceRequest();
        return JsonSerializer.Deserialize<DynamicServiceRequest>(json, PayloadJson)
            ?? new DynamicServiceRequest();
    }
}
