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

    // ── Dock CRUD ─────────────────────────────────────────────

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
            ?? throw new NotFoundException("Bến tàu không tồn tại");

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
            ?? throw new NotFoundException("Bến tàu không tồn tại");

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
            ?? throw new NotFoundException("Bến tàu không tồn tại");

        await _dockRepository.DeleteAsync(dock);
    }

    // ── Schedules ─────────────────────────────────────────────

    public async Task<List<DockScheduleResponse>> GetSchedulesAsync(Guid dockId)
    {
        await EnsureDockExistsAsync(dockId);
        var schedules = await _dockRepository.GetSchedulesAsync(dockId);
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public async Task<DockScheduleResponse> AddScheduleAsync(Guid dockId, CreateDockScheduleRequest request)
    {
        var dock = await _dockRepository.GetByIdAsync(dockId)
            ?? throw new NotFoundException("Bến tàu không tồn tại");

        var errors = new Dictionary<string, List<string>>();

        if (request.endTime <= request.startTime)
            errors["endTime"] = ["Thời gian kết thúc phải sau thời gian bắt đầu"];

        if (errors.Count > 0)
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, errors);

        // Kiểm tra cùng thuyền đã có lịch trùng thời gian tại bến này chưa
        var hasConflict = await _dockRepository.HasConflictAsync(dockId, request.boatId, request.startTime, request.endTime);
        if (hasConflict)
            throw new AppException(ErrorCode.AuthValidationFailed,
                "Thuyền này đã có lịch trùng thời gian tại bến");

        // Kiểm tra bến có đủ chỗ tại thời điểm bắt đầu không
        var activeAtStart = await _dockRepository.CountActiveSchedulesAsync(dockId, request.startTime);
        if (activeAtStart >= dock.max_boats)
            throw new AppException(ErrorCode.AuthValidationFailed,
                $"Bến đã đầy ({dock.max_boats} thuyền) trong khoảng thời gian này");

        var schedule = new dock_schedule
        {
            id = Guid.NewGuid(),
            dock_id = dockId,
            boat_id = request.boatId,
            schedule_id = request.scheduleId,
            start_time = request.startTime,
            end_time = request.endTime,
            created_at = DateTime.UtcNow,
        };

        await _dockRepository.AddScheduleAsync(schedule);

        var saved = await _dockRepository.GetScheduleAsync(dockId, schedule.id);
        return MapToScheduleResponse(saved!);
    }

    public async Task DeleteScheduleAsync(Guid dockId, Guid scheduleId)
    {
        await EnsureDockExistsAsync(dockId);
        var schedule = await _dockRepository.GetScheduleAsync(dockId, scheduleId)
            ?? throw new NotFoundException("Lịch bến không tồn tại");
        await _dockRepository.DeleteScheduleAsync(schedule);
    }

    // ── Helpers ───────────────────────────────────────────────

    private async Task EnsureDockExistsAsync(Guid dockId)
    {
        var dock = await _dockRepository.GetByIdAsync(dockId);
        if (dock is null) throw new NotFoundException("Bến tàu không tồn tại");
    }

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

    private static DockScheduleResponse MapToScheduleResponse(dock_schedule s) => new()
    {
        id = s.id,
        dockId = s.dock_id,
        boatId = s.boat_id,
        boatName = s.boat?.name ?? string.Empty,
        scheduleId = s.schedule_id,
        startTime = s.start_time,
        endTime = s.end_time,
        createdAt = s.created_at,
    };
}
