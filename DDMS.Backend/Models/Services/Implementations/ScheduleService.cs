using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ITourRepository _tourRepository;

    public ScheduleService(IScheduleRepository scheduleRepository, ITourRepository tourRepository)
    {
        _scheduleRepository = scheduleRepository;
        _tourRepository = tourRepository;
    }

    public async Task<PagedResponse<ScheduleItemResponse>> GetSchedulesAsync(Guid userId, ScheduleListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            if (!TourConstants.ScheduleStatuses.Allowed.Contains(normalizedStatus))
            {
                throw new AppException(ErrorCode.ScheduleStatusInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
                {
                    ["status"] = [ErrorCode.Messages.ScheduleStatusInvalid]
                });
            }
        }

        var (items, total) = await _scheduleRepository.GetPagedAsync(userId, query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        return new PagedResponse<ScheduleItemResponse>
        {
            items = items.Select(MapSchedule).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<ScheduleItemResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var entity = await _scheduleRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapSchedule(entity);
    }

    public async Task<ScheduleItemResponse> CreateAsync(Guid userId, CreateScheduleRequest request)
    {
        ValidateTimeRange(request.startTime, request.endTime);

        var tourEntity = await _tourRepository.GetByIdAsync(request.tourId, userId);
        if (tourEntity is null)
        {
            throw new AppException(ErrorCode.ScheduleTourNotFound, ErrorCode.Messages.ScheduleTourNotFound);
        }

        await ValidateBoatDockReferences(request.boatId, request.dockId);

        var entity = new tour_schedule
        {
            id = Guid.NewGuid(),
            tour_id = request.tourId,
            boat_id = request.boatId,
            dock_id = request.dockId,
            start_time = request.startTime,
            end_time = request.endTime,
            status = TourConstants.ScheduleStatuses.Scheduled,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _scheduleRepository.AddAsync(entity);
        var created = await _scheduleRepository.GetByIdAsync(entity.id, userId);
        return MapSchedule(created!);
    }

    public async Task<ScheduleItemResponse> UpdateAsync(Guid id, Guid userId, UpdateScheduleRequest request)
    {
        ValidateTimeRange(request.startTime, request.endTime);

        var normalizedStatus = request.status.Trim().ToLowerInvariant();
        if (!TourConstants.ScheduleStatuses.Allowed.Contains(normalizedStatus))
        {
            throw new AppException(ErrorCode.ScheduleStatusInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
            {
                ["status"] = [ErrorCode.Messages.ScheduleStatusInvalid]
            });
        }

        var tourEntity = await _tourRepository.GetByIdAsync(request.tourId, userId);
        if (tourEntity is null)
        {
            throw new AppException(ErrorCode.ScheduleTourNotFound, ErrorCode.Messages.ScheduleTourNotFound);
        }

        await ValidateBoatDockReferences(request.boatId, request.dockId);

        var entity = await _scheduleRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.tour_id = request.tourId;
        entity.boat_id = request.boatId;
        entity.dock_id = request.dockId;
        entity.start_time = request.startTime;
        entity.end_time = request.endTime;
        entity.status = normalizedStatus;

        await _scheduleRepository.UpdateAsync(entity);
        var updated = await _scheduleRepository.GetByIdAsync(id, userId);
        return MapSchedule(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var entity = await _scheduleRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.status = TourConstants.ScheduleStatuses.Cancelled;
        await _scheduleRepository.UpdateAsync(entity);
    }

    private async Task ValidateBoatDockReferences(Guid? boatId, Guid? dockId)
    {
        if (boatId.HasValue && !await _scheduleRepository.BoatExistsAsync(boatId.Value))
        {
            throw new AppException(ErrorCode.ScheduleBoatNotFound, ErrorCode.Messages.ScheduleBoatNotFound);
        }

        if (dockId.HasValue && !await _scheduleRepository.DockExistsAsync(dockId.Value))
        {
            throw new AppException(ErrorCode.ScheduleDockNotFound, ErrorCode.Messages.ScheduleDockNotFound);
        }
    }

    private static void ValidateTimeRange(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new AppException(ErrorCode.ScheduleTimeInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
            {
                ["endTime"] = [ErrorCode.Messages.ScheduleTimeInvalid]
            });
        }
    }

    private static ScheduleItemResponse MapSchedule(tour_schedule entity)
    {
        return new ScheduleItemResponse
        {
            id = entity.id,
            tourId = entity.tour_id,
            tourName = entity.tour.name,
            boatId = entity.boat_id,
            boatName = entity.boat?.name,
            dockId = entity.dock_id,
            dockName = entity.dock?.name,
            startTime = entity.start_time,
            endTime = entity.end_time,
            status = entity.status,
            createdAt = entity.created_at,
            updatedAt = entity.updated_at
        };
    }
}
