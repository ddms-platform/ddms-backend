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
        // Thuyền phải thuộc về chính người gọi. Trước đây hàm chỉ đọc owner_id
        // TỪ CON THUYỀN rồi dùng luôn, không đối chiếu với ai đang gọi — nên chủ
        // thuyền A đăng ký được tour trên thuyền của chủ thuyền B.
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
        }

        var now = DateTime.UtcNow;
        TourResponse tourResponse;

        if (existingTour != null)
        {
            // Cập nhật tour hiện tại, không tạo tour mới
            existingTour.name = request.name.Trim();
            existingTour.price = request.basePrice;
            if (request.childPricePercent is not null)
                existingTour.child_price_percent = request.childPricePercent.Value;
            if (request.infantPricePercent is not null)
                existingTour.infant_price_percent = request.infantPricePercent.Value;
            existingTour.description = BuildDescription(request);
            existingTour.service_type = NormalizeServiceType(request.serviceType);
            existingTour.status = OwnerServiceRegistrationDefaults.TourPendingStatus; // Chuyển về pending để Admin duyệt lại nội dung cập nhật
            existingTour.updated_at = now;
            if (existingTour.created_by == null && ownerId.HasValue)
            {
                existingTour.created_by = ownerId.Value;
            }

            // Đồng bộ lại FAQs
            await _repo.RemoveFaqsByTourIdAsync(existingTour.id, ct);
            AddFaqs(request, existingTour.id, now);

            // Đồng bộ lại Routes
            await _repo.RemoveRoutesByTourIdAsync(existingTour.id, ct);
            AddRoutes(request, existingTour.id, now);

            if (request.imageUrls != null)
            {
                await _repo.RemoveImagesByTourIdAsync(existingTour.id, ct);
                AddTourImages(request, existingTour.id, now);
            }

            // Đồng bộ Cabins / Combos của RIÊNG tour này.
            // Trước đây xoá theo boatId — nghĩa là sửa một dịch vụ thì mọi tour
            // khác chạy trên cùng con thuyền mất sạch hạng phòng và combo.
            if (request.rooms != null && request.rooms.Count > 0)
            {
                await _repo.RemoveCabinsByTourIdAsync(existingTour.id, ct);
                AddCabins(request, existingTour.id, now);
            }
            if (request.combos != null && request.combos.Count > 0)
            {
                await _repo.RemoveCombosByTourIdAsync(existingTour.id, ct);
                AddCombos(request, existingTour.id, now);
            }

            // Đảm bảo có lịch trình gắn với tàu
            if (!await _repo.HasScheduleForBoatAndTourAsync(request.boatId, existingTour.id, ct))
            {
                AddDefaultSchedule(request.boatId, existingTour.id, now);
            }

            await _repo.SaveChangesAsync(ct);
            tourResponse = new TourResponse
            {
                id = existingTour.id,
                name = existingTour.name,
                description = existingTour.description,
                price = existingTour.price,
                duration_minutes = existingTour.duration_minutes,
                location = existingTour.location,
                avg_rating = existingTour.avg_rating,
                total_reviews = existingTour.total_reviews,
                status = existingTour.status,
                cancel_policy = existingTour.cancel_policy,
                cancel_hours = existingTour.cancel_hours
            };
        }
        else
        {
            // Tạo mới tour
            var createTourReq = new CreateTourRequest
            {
                name = request.name,
                price = request.basePrice,
                child_price_percent = request.childPricePercent,
                infant_price_percent = request.infantPricePercent,
                description = BuildDescription(request),
                duration_minutes = OwnerServiceRegistrationDefaults.TourDurationMinutes,
                location = OwnerServiceRegistrationDefaults.TourLocation,
                service_type = NormalizeServiceType(request.serviceType),
                status = OwnerServiceRegistrationDefaults.TourPendingStatus,
                cancel_policy = OwnerServiceRegistrationDefaults.CancelPolicy,
                created_by = ownerId
            };

            var tour = await _tourService.CreateAsync(createTourReq, ct);
            AddCabins(request, tour.id, now);
            AddCombos(request, tour.id, now);
            AddFaqs(request, tour.id, now);
            AddRoutes(request, tour.id, now);
            AddTourImages(request, tour.id, now);
            AddDefaultSchedule(request.boatId, tour.id, now);

            await _repo.SaveChangesAsync(ct);
            tourResponse = tour;
        }

        await TrySendConfirmationEmailAsync(request, ct);
        return tourResponse;
    }

    /// <summary>
    /// Chỉ nhận các loại dịch vụ hệ thống biết. Trước đây giá trị này bị vứt
    /// hoàn toàn nên mở lại form là dịch vụ nào cũng thành "cruise".
    /// </summary>
    private static string? NormalizeServiceType(string? raw)
    {
        var value = raw?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(value) || !ServiceTypes.IsValid(value) ? null : value;
    }

    private static string BuildDescription(DynamicServiceRequest r)
    {
        var s = r.description ?? string.Empty;
        if (!string.IsNullOrEmpty(r.route))       s += $"\n| Lộ trình: {r.route}";
        if (!string.IsNullOrEmpty(r.equipments))  s += $"\n| Dụng cụ: {r.equipments}";
        if (r.pricePerDay.HasValue)               s += $"\n| Giá nguyên ngày: {r.pricePerDay} VNĐ";
        return s;
    }

    private void AddCabins(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        foreach (var r in req.rooms ?? Enumerable.Empty<ServiceRoom>())
        {
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

    private void AddTourImages(DynamicServiceRequest req, Guid tourId, DateTime now)
    {
        var sort = 0;
        foreach (var rawUrl in req.imageUrls ?? Enumerable.Empty<string>())
        {
            var url = rawUrl?.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

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
