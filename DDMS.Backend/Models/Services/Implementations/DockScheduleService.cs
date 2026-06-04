using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class DockScheduleService : IDockScheduleService
{
    private readonly IDockScheduleRepository _dockScheduleRepository;

    public DockScheduleService(IDockScheduleRepository dockScheduleRepository)
    {
        _dockScheduleRepository = dockScheduleRepository;
    }

    public async Task<PagedResponse<DockScheduleItemResponse>> GetSchedulesAsync(DockScheduleListQuery query)
    {
        var (items, total) = await _dockScheduleRepository.GetPagedAsync(query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        return new PagedResponse<DockScheduleItemResponse>
        {
            items = items.Select(MapSchedule).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<DockScheduleItemResponse> GetByIdAsync(Guid id)
    {
        var entity = await _dockScheduleRepository.GetByIdAsync(id);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapSchedule(entity);
    }

    public async Task<DockScheduleItemResponse> CreateAsync(CreateDockScheduleRequest request)
    {
        await ValidateReferencesAndOverlapAsync(
            request.dockId,
            request.boatId,
            request.startTime,
            request.endTime);

        var entity = new dock_schedule
        {
            id = Guid.NewGuid(),
            dock_id = request.dockId,
            boat_id = request.boatId,
            schedule_id = request.scheduleId,
            start_time = request.startTime,
            end_time = request.endTime,
            created_at = DateTime.UtcNow
        };

        await _dockScheduleRepository.AddAsync(entity);
        var created = await _dockScheduleRepository.GetByIdAsync(entity.id);
        return MapSchedule(created!);
    }

    public async Task<DockScheduleItemResponse> UpdateAsync(Guid id, UpdateDockScheduleRequest request)
    {
        var entity = await _dockScheduleRepository.GetByIdAsync(id);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        await ValidateReferencesAndOverlapAsync(
            request.dockId,
            request.boatId,
            request.startTime,
            request.endTime,
            id);

        entity.dock_id = request.dockId;
        entity.boat_id = request.boatId;
        entity.schedule_id = request.scheduleId;
        entity.start_time = request.startTime;
        entity.end_time = request.endTime;

        await _dockScheduleRepository.UpdateAsync(entity);
        var updated = await _dockScheduleRepository.GetByIdAsync(id);
        return MapSchedule(updated!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dockScheduleRepository.GetByIdAsync(id);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        await _dockScheduleRepository.DeleteAsync(entity);
    }

    private async Task ValidateReferencesAndOverlapAsync(
        Guid dockId,
        Guid boatId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeId = null)
    {
        if (endTime <= startTime)
        {
            throw new AppException(ErrorCode.DockScheduleTimeInvalid, ErrorCode.Messages.TourValidationFailed,
                new Dictionary<string, List<string>>
                {
                    ["endTime"] = [ErrorCode.Messages.DockScheduleTimeInvalid]
                });
        }

        var dock = await _dockScheduleRepository.GetDockAsync(dockId);
        if (dock is null)
        {
            throw new AppException(ErrorCode.DockScheduleDockNotFound, ErrorCode.Messages.DockScheduleDockNotFound);
        }

        var boat = await _dockScheduleRepository.GetBoatAsync(boatId);
        if (boat is null)
        {
            throw new AppException(ErrorCode.DockScheduleBoatNotFound, ErrorCode.Messages.DockScheduleBoatNotFound);
        }

        if (await _dockScheduleRepository.HasBoatOverlapAsync(boatId, startTime, endTime, excludeId))
        {
            throw new AppException(ErrorCode.DockScheduleOverlap, ErrorCode.Messages.DockScheduleBoatOverlap);
        }

        var overlappingAtDock = await _dockScheduleRepository.CountDockOverlapsAsync(dockId, startTime, endTime, excludeId);
        if (overlappingAtDock >= dock.max_boats)
        {
            throw new AppException(
                ErrorCode.DockScheduleDockCapacityExceeded,
                ErrorCode.Messages.DockScheduleDockCapacityExceeded);
        }
    }

    private static DockScheduleItemResponse MapSchedule(dock_schedule entity)
    {
        return new DockScheduleItemResponse
        {
            id = entity.id,
            dockId = entity.dock_id,
            dockName = entity.dock.name,
            boatId = entity.boat_id,
            boatName = entity.boat.name,
            scheduleId = entity.schedule_id,
            startTime = entity.start_time,
            endTime = entity.end_time,
            createdAt = entity.created_at
        };
    }
}
