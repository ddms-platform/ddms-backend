using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class BoatCabinService : IBoatCabinService
{
    private readonly IBoatCabinRepository _cabinRepository;
    private readonly IBoatRepository _boatRepository;

    public BoatCabinService(IBoatCabinRepository cabinRepository, IBoatRepository boatRepository)
    {
        _cabinRepository = cabinRepository;
        _boatRepository = boatRepository;
    }

    public async Task<List<BoatCabinResponse>> GetByBoatIdAsync(Guid boatId)
    {
        await EnsureBoatExistsAsync(boatId);
        var cabins = await _cabinRepository.GetByBoatIdAsync(boatId);
        return cabins.Select(MapToResponse).ToList();
    }

    public async Task<BoatCabinResponse> GetByIdAsync(Guid boatId, Guid id)
    {
        var cabin = await GetCabinAndValidateOwnership(boatId, id);
        return MapToResponse(cabin);
    }

    public async Task<BoatCabinResponse> CreateAsync(Guid boatId, CreateBoatCabinRequest request)
    {
        await EnsureBoatExistsAsync(boatId);
        ValidateRequest(request.name, request.capacity, request.price, request.totalRooms);

        var cabin = new boat_cabin
        {
            id = Guid.NewGuid(),
            boat_id = boatId,
            name = request.name.Trim(),
            capacity = request.capacity,
            price = request.price,
            total_rooms = request.totalRooms,
            description = string.IsNullOrWhiteSpace(request.description) ? null : request.description.Trim(),
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow,
        };

        await _cabinRepository.CreateAsync(cabin);
        return MapToResponse(cabin);
    }

    public async Task<BoatCabinResponse> UpdateAsync(Guid boatId, Guid id, UpdateBoatCabinRequest request)
    {
        var cabin = await GetCabinAndValidateOwnership(boatId, id);
        ValidateRequest(request.name, request.capacity, request.price, request.totalRooms);

        cabin.name = request.name.Trim();
        cabin.capacity = request.capacity;
        cabin.price = request.price;
        cabin.total_rooms = request.totalRooms;
        cabin.description = string.IsNullOrWhiteSpace(request.description) ? null : request.description.Trim();
        cabin.updated_at = DateTime.UtcNow;

        await _cabinRepository.UpdateAsync(cabin);
        return MapToResponse(cabin);
    }

    public async Task DeleteAsync(Guid boatId, Guid id)
    {
        var cabin = await GetCabinAndValidateOwnership(boatId, id);
        await _cabinRepository.DeleteAsync(cabin);
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task EnsureBoatExistsAsync(Guid boatId)
    {
        var boat = await _boatRepository.GetByIdAsync(boatId);
        if (boat is null) throw new NotFoundException("Thuyền không tồn tại");
    }

    private async Task<boat_cabin> GetCabinAndValidateOwnership(Guid boatId, Guid id)
    {
        await EnsureBoatExistsAsync(boatId);
        var cabin = await _cabinRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Cabin không tồn tại");

        if (cabin.boat_id != boatId)
            throw new ForbiddenException("Cabin không thuộc thuyền này");

        return cabin;
    }

    private static void ValidateRequest(string name, int capacity, decimal price, int totalRooms)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Tên cabin là bắt buộc"];

        if (capacity < 1)
            errors["capacity"] = ["Sức chứa phải lớn hơn 0"];

        if (price < 0)
            errors["price"] = ["Giá không thể âm"];

        if (totalRooms < 1)
            errors["totalRooms"] = ["Số phòng phải lớn hơn 0"];

        if (errors.Count > 0)
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, errors);
    }

    private static BoatCabinResponse MapToResponse(boat_cabin c) => new()
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
    };
}
