using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class BoatService : IBoatService
{
    private static readonly HashSet<string> AllowedStatuses = ["idle", "running"];
    private static readonly HashSet<string> AllowedTypes = ["catamaran", "fishing_boat", "speedboat", "cruiser", "yacht", "cruise", "luxury", "standard", "party"];

    private readonly IBoatRepository _boatRepository;

    public BoatService(IBoatRepository boatRepository)
    {
        _boatRepository = boatRepository;
    }

    public async Task<PagedResponse<BoatListItemResponse>> GetBoatsAsync(BoatListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var (items, total) = await _boatRepository.GetPagedAsync(query);

        return new PagedResponse<BoatListItemResponse>
        {
            items = items.Select(MapToListItem).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<List<BoatListItemResponse>> GetAllAsync()
    {
        var boats = await _boatRepository.GetAllAsync();
        return boats.Select(MapToListItem).ToList();
    }

    public async Task<BoatDetailResponse> GetByIdAsync(Guid id)
    {
        var boat = await _boatRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException();

        return MapToDetail(boat);
    }

    public Task<BoatStatsResponse> GetStatsAsync()
    {
        return _boatRepository.GetStatsAsync();
    }

    public async Task<BoatDetailResponse> CreateAsync(CreateBoatRequest request)
    {
        ValidateBoatRequest(request.name, request.maxPassengers, request.status, request.type);

        var boat = new boat
        {
            id = Guid.NewGuid(),
            name = request.name.Trim(),
            type = string.IsNullOrWhiteSpace(request.type) ? null : request.type.Trim().ToLowerInvariant(),
            max_passengers = request.maxPassengers,
            status = string.IsNullOrWhiteSpace(request.status) ? "idle" : request.status.Trim().ToLowerInvariant(),
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow,
        };

        await _boatRepository.CreateAsync(boat);

        // Reload with details
        var created = await _boatRepository.GetByIdWithDetailsAsync(boat.id)!;
        return MapToDetail(created!);
    }

    public async Task<BoatDetailResponse> UpdateAsync(Guid id, UpdateBoatRequest request)
    {
        var boat = await _boatRepository.GetByIdAsync(id)
            ?? throw new NotFoundException();

        ValidateBoatRequest(request.name, request.maxPassengers, request.status, request.type);

        boat.name = request.name.Trim();
        boat.type = string.IsNullOrWhiteSpace(request.type) ? null : request.type.Trim().ToLowerInvariant();
        boat.max_passengers = request.maxPassengers;
        boat.status = string.IsNullOrWhiteSpace(request.status) ? "idle" : request.status.Trim().ToLowerInvariant();
        boat.updated_at = DateTime.UtcNow;

        await _boatRepository.UpdateAsync(boat);

        var updated = await _boatRepository.GetByIdWithDetailsAsync(id)!;
        return MapToDetail(updated!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var boat = await _boatRepository.GetByIdAsync(id)
            ?? throw new NotFoundException();

        await _boatRepository.DeleteAsync(boat);
    }

    // ── Owner-specific ────────────────────────────────────────

    public async Task<PagedResponse<BoatListItemResponse>> GetBoatsByOwnerAsync(Guid ownerId, OwnerBoatListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var (items, total) = await _boatRepository.GetPagedByOwnerAsync(ownerId, query);

        return new PagedResponse<BoatListItemResponse>
        {
            items = items.Select(MapToListItem).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<BoatDetailResponse> GetByIdByOwnerAsync(Guid id, Guid ownerId)
    {
        var boat = await _boatRepository.GetByIdAndOwnerAsync(id, ownerId)
            ?? throw new NotFoundException("Thuyền không tồn tại hoặc bạn không có quyền truy cập");
        return MapToDetail(boat);
    }

    public Task<BoatStatsResponse> GetStatsByOwnerAsync(Guid ownerId)
    {
        return _boatRepository.GetStatsByOwnerAsync(ownerId);
    }

    public async Task<BoatDetailResponse> CreateByOwnerAsync(CreateBoatRequest request, Guid ownerId)
    {
        ValidateBoatRequest(request.name, request.maxPassengers, request.status, request.type);

        var boat = new boat
        {
            id = Guid.NewGuid(),
            owner_id = ownerId,
            name = request.name.Trim(),
            type = string.IsNullOrWhiteSpace(request.type) ? null : request.type.Trim().ToLowerInvariant(),
            max_passengers = request.maxPassengers,
            status = string.IsNullOrWhiteSpace(request.status) ? "idle" : request.status.Trim().ToLowerInvariant(),
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow,
        };

        await _boatRepository.CreateAsync(boat);
        var created = await _boatRepository.GetByIdWithDetailsAsync(boat.id)!;
        return MapToDetail(created!);
    }

    public async Task<BoatDetailResponse> UpdateByOwnerAsync(Guid id, UpdateBoatRequest request, Guid ownerId)
    {
        var boat = await _boatRepository.GetByIdAndOwnerAsync(id, ownerId)
            ?? throw new NotFoundException("Thuyền không tồn tại hoặc bạn không có quyền chỉnh sửa");

        ValidateBoatRequest(request.name, request.maxPassengers, request.status, request.type);

        boat.name = request.name.Trim();
        boat.type = string.IsNullOrWhiteSpace(request.type) ? null : request.type.Trim().ToLowerInvariant();
        boat.max_passengers = request.maxPassengers;
        boat.status = string.IsNullOrWhiteSpace(request.status) ? "idle" : request.status.Trim().ToLowerInvariant();
        boat.updated_at = DateTime.UtcNow;

        await _boatRepository.UpdateAsync(boat);
        var updated = await _boatRepository.GetByIdWithDetailsAsync(id)!;
        return MapToDetail(updated!);
    }

    public async Task DeleteByOwnerAsync(Guid id, Guid ownerId)
    {
        var boat = await _boatRepository.GetByIdAndOwnerAsync(id, ownerId)
            ?? throw new NotFoundException("Thuyền không tồn tại hoặc bạn không có quyền xóa");

        await _boatRepository.DeleteAsync(boat);
    }

    // ── Helpers ──────────────────────────────────────────────

    private static void ValidateBoatRequest(string name, int maxPassengers, string? status, string? type)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Tên thuyền là bắt buộc"];

        if (maxPassengers < 1)
            errors["maxPassengers"] = ["Sức chứa phải lớn hơn 0"];

        if (!string.IsNullOrWhiteSpace(status) && !AllowedStatuses.Contains(status.Trim().ToLowerInvariant()))
            errors["status"] = ["Trạng thái không hợp lệ (idle, running)"];

        if (!string.IsNullOrWhiteSpace(type) && !AllowedTypes.Contains(type.Trim().ToLowerInvariant()))
            errors["type"] = ["Loại thuyền không hợp lệ (cruise, luxury, standard, party, speedboat)"];

        if (errors.Count > 0)
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, errors);
    }

    private static string CalculateStatus(boat b)
    {
        var now = DateTime.UtcNow;
        if (b.boat_maintenances != null && b.boat_maintenances.Any(m => m.start_time <= now && now <= m.end_time))
        {
            return "maintenance";
        }
        if (b.tour_schedules != null && b.tour_schedules.Any(ts => ts.start_time <= now && now <= ts.end_time && (ts.status == "ongoing" || ts.status == "scheduled")))
        {
            return "running";
        }
        return "idle";
    }

    private static BoatListItemResponse MapToListItem(boat b) => new()
    {
        id = b.id,
        ownerId = b.owner_id,
        name = b.name,
        type = b.type,
        maxPassengers = b.max_passengers,
        status = CalculateStatus(b),
        cabinCount = b.boat_cabins?.Count ?? 0,
        serviceCount = b.boat_services?.Count ?? 0,
        thumbnailUrl = b.boat_images?.OrderBy(i => i.sort_order).FirstOrDefault()?.image_url,
        createdAt = b.created_at,
        updatedAt = b.updated_at,
    };

    private static BoatDetailResponse MapToDetail(boat b) => new()
    {
        id = b.id,
        name = b.name,
        type = b.type,
        maxPassengers = b.max_passengers,
        status = CalculateStatus(b),
        createdAt = b.created_at,
        updatedAt = b.updated_at,
        cabins = b.boat_cabins?.Select(c => new BoatCabinResponse
        {
            id = c.id,
            boatId = c.boat_id,
            name = c.name,
            capacity = c.capacity,
            price = c.price,
            totalRooms = c.total_rooms,
            description = c.description,
            createdAt = c.created_at,
            updatedAt = c.updated_at,
        }).ToList() ?? [],
        services = (b.boat_services?.Select(s => new BoatServiceResponse
        {
            id = s.id,
            boatId = s.boat_id,
            name = s.name,
            price = s.price,
            description = s.description,
            isActive = s.is_active ?? true,
            createdAt = s.created_at,
            updatedAt = s.updated_at,
        }) ?? Enumerable.Empty<BoatServiceResponse>())
        .Concat(b.tour_schedules?.Select(ts => ts.tour).Where(t => t != null).Select(t => new BoatServiceResponse
        {
            id = t.id,
            boatId = b.id,
            name = t.name,
            price = t.price,
            description = t.description,
            isActive = t.status == "active",
            createdAt = t.created_at,
            updatedAt = t.updated_at,
        }) ?? Enumerable.Empty<BoatServiceResponse>()).ToList(),
        images = b.boat_images?.OrderBy(i => i.sort_order).Select(i => new BoatImageResponse
        {
            id = i.id,
            boatId = i.boat_id,
            imageUrl = i.image_url,
            publicId = i.public_id,
            caption = i.caption,
            sortOrder = i.sort_order,
            createdAt = i.created_at,
        }).ToList() ?? [],
        maintenances = b.boat_maintenances?.Select(m => new BoatMaintenanceResponse
        {
            id = m.id,
            boatId = m.boat_id,
            startTime = m.start_time,
            endTime = m.end_time,
            reason = m.reason,
            createdAt = m.created_at,
            portMaintenanceServiceId = m.port_maintenance_service_id,
            status = m.status,
            portMaintenanceServiceName = m.port_maintenance_service?.name
        }).ToList() ?? [],
    };
}
