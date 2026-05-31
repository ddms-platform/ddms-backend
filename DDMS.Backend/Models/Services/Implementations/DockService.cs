using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class DockService : IDockService
{
    private readonly IDockRepository _dockRepository;

    public DockService(IDockRepository dockRepository)
    {
        _dockRepository = dockRepository;
    }

    public async Task<PagedResponse<DockListItemResponse>> GetDocksAsync(DockListQuery query)
    {
        var page = query.page < 1 ? 1 : query.page;
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;

        var (items, total) = await _dockRepository.GetPagedAsync(query);

        return new PagedResponse<DockListItemResponse>
        {
            items = items.Select(MapToResponse).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    public async Task<List<DockListItemResponse>> GetAllAsync()
    {
        var docks = await _dockRepository.GetAllAsync();
        return docks.Select(MapToResponse).ToList();
    }

    public async Task<DockListItemResponse> GetByIdAsync(Guid id)
    {
        var dock = await _dockRepository.GetByIdAsync(id)
            ?? throw new NotFoundException();

        return MapToResponse(dock);
    }

    public Task<DockStatsResponse> GetStatsAsync()
    {
        return _dockRepository.GetStatsAsync();
    }

    public async Task<DockListItemResponse> CreateAsync(CreateDockRequest request)
    {
        ValidateRequest(request.name, request.maxBoats);

        var dock = new dock
        {
            id = Guid.NewGuid(),
            name = request.name.Trim(),
            location = string.IsNullOrWhiteSpace(request.location) ? null : request.location.Trim(),
            max_boats = request.maxBoats,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow,
        };

        await _dockRepository.CreateAsync(dock);
        return MapToResponse(dock);
    }

    public async Task<DockListItemResponse> UpdateAsync(Guid id, UpdateDockRequest request)
    {
        var dock = await _dockRepository.GetByIdAsync(id)
            ?? throw new NotFoundException();

        ValidateRequest(request.name, request.maxBoats);

        dock.name = request.name.Trim();
        dock.location = string.IsNullOrWhiteSpace(request.location) ? null : request.location.Trim();
        dock.max_boats = request.maxBoats;
        dock.updated_at = DateTime.UtcNow;

        await _dockRepository.UpdateAsync(dock);
        return MapToResponse(dock);
    }

    public async Task DeleteAsync(Guid id)
    {
        var dock = await _dockRepository.GetByIdAsync(id)
            ?? throw new NotFoundException();

        await _dockRepository.DeleteAsync(dock);
    }

    // ── Helpers ──────────────────────────────────────────────

    private static void ValidateRequest(string name, int maxBoats)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Tên bến tàu là bắt buộc"];

        if (maxBoats < 1)
            errors["maxBoats"] = ["Sức chứa thuyền phải lớn hơn 0"];

        if (errors.Count > 0)
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, errors);
    }

    private static DockListItemResponse MapToResponse(dock d) => new()
    {
        id = d.id,
        name = d.name,
        location = d.location,
        maxBoats = d.max_boats,
        createdAt = d.created_at,
        updatedAt = d.updated_at,
    };
}
