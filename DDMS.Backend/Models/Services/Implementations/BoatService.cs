using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class BoatService : IBoatService
{
    private static readonly HashSet<string> AllowedStatuses = ["idle", "running"];
    private static readonly HashSet<string> AllowedTypes = ["cruise", "luxury", "standard", "party", "speedboat"];

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
            status = request.status.Trim().ToLowerInvariant(),
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
        boat.status = request.status.Trim().ToLowerInvariant();
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

    // ── Helpers ──────────────────────────────────────────────

    private static void ValidateBoatRequest(string name, int maxPassengers, string status, string? type)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Tên thuyền là bắt buộc"];

        if (maxPassengers < 1)
            errors["maxPassengers"] = ["Sức chứa phải lớn hơn 0"];

        if (!AllowedStatuses.Contains(status?.Trim().ToLowerInvariant() ?? ""))
            errors["status"] = ["Trạng thái không hợp lệ (idle, running)"];

        if (!string.IsNullOrWhiteSpace(type) && !AllowedTypes.Contains(type.Trim().ToLowerInvariant()))
            errors["type"] = ["Loại thuyền không hợp lệ (cruise, luxury, standard, party, speedboat)"];

        if (errors.Count > 0)
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, errors);
    }

    private static BoatListItemResponse MapToListItem(boat b) => new()
    {
        id = b.id,
        name = b.name,
        type = b.type,
        maxPassengers = b.max_passengers,
        status = b.status,
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
        status = b.status,
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
        services = b.boat_services?.Select(s => new BoatServiceResponse
        {
            id = s.id,
            boatId = s.boat_id,
            name = s.name,
            price = s.price,
            description = s.description,
            isActive = s.is_active ?? true,
            createdAt = s.created_at,
            updatedAt = s.updated_at,
        }).ToList() ?? [],
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
        }).ToList() ?? [],
    };
}
