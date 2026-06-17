using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class BoatAddonService : IBoatAddonService
{
    private readonly IBoatAddonRepository _addonRepository;
    private readonly IBoatRepository _boatRepository;

    public BoatAddonService(IBoatAddonRepository addonRepository, IBoatRepository boatRepository)
    {
        _addonRepository = addonRepository;
        _boatRepository = boatRepository;
    }

    public async Task<List<BoatServiceResponse>> GetByBoatIdAsync(Guid boatId)
    {
        await EnsureBoatExistsAsync(boatId);
        var services = await _addonRepository.GetByBoatIdAsync(boatId);
        return services.Select(MapToResponse).ToList();
    }

    public async Task<BoatServiceResponse> GetByIdAsync(Guid boatId, Guid id)
    {
        var addon = await GetAddonAndValidateOwnership(boatId, id);
        return MapToResponse(addon);
    }

    public async Task<BoatServiceResponse> CreateAsync(Guid boatId, CreateBoatServiceRequest request)
    {
        await EnsureBoatExistsAsync(boatId);
        ValidateRequest(request.name, request.price);

        var addon = new boat_service
        {
            id = Guid.NewGuid(),
            boat_id = boatId,
            name = request.name.Trim(),
            price = request.price,
            description = string.IsNullOrWhiteSpace(request.description) ? null : request.description.Trim(),
            is_active = request.isActive,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow,
        };

        await _addonRepository.CreateAsync(addon);
        return MapToResponse(addon);
    }

    public async Task<BoatServiceResponse> UpdateAsync(Guid boatId, Guid id, UpdateBoatServiceRequest request)
    {
        var addon = await GetAddonAndValidateOwnership(boatId, id);
        ValidateRequest(request.name, request.price);

        addon.name = request.name.Trim();
        addon.price = request.price;
        addon.description = string.IsNullOrWhiteSpace(request.description) ? null : request.description.Trim();
        addon.is_active = request.isActive;
        addon.updated_at = DateTime.UtcNow;

        await _addonRepository.UpdateAsync(addon);
        return MapToResponse(addon);
    }

    public async Task<BoatServiceResponse> ToggleActiveAsync(Guid boatId, Guid id)
    {
        var addon = await GetAddonAndValidateOwnership(boatId, id);
        addon.is_active = !(addon.is_active ?? true);
        addon.updated_at = DateTime.UtcNow;

        await _addonRepository.UpdateAsync(addon);
        return MapToResponse(addon);
    }

    public async Task DeleteAsync(Guid boatId, Guid id)
    {
        var addon = await GetAddonAndValidateOwnership(boatId, id);
        await _addonRepository.DeleteAsync(addon);
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task EnsureBoatExistsAsync(Guid boatId)
    {
        var boat = await _boatRepository.GetByIdAsync(boatId);
        if (boat is null) throw new NotFoundException("Thuyền không tồn tại");
    }

    private async Task<boat_service> GetAddonAndValidateOwnership(Guid boatId, Guid id)
    {
        await EnsureBoatExistsAsync(boatId);
        var addon = await _addonRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Dịch vụ không tồn tại");

        if (addon.boat_id != boatId)
            throw new ForbiddenException("Dịch vụ không thuộc thuyền này");

        return addon;
    }

    private static void ValidateRequest(string name, decimal price)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Tên dịch vụ là bắt buộc"];

        if (price < 0)
            errors["price"] = ["Giá không thể âm"];

        if (errors.Count > 0)
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, errors);
    }

    private static BoatServiceResponse MapToResponse(boat_service s) => new()
    {
        id = s.id,
        boatId = s.boat_id,
        name = s.name,
        price = s.price,
        description = s.description,
        isActive = s.is_active ?? true,
        createdAt = s.created_at,
        updatedAt = s.updated_at,
    };
}
