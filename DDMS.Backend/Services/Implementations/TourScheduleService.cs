using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Models.DTOs.TourSchedule;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourScheduleService : ITourScheduleService
{
    private readonly ITourScheduleRepository _repository;
    private readonly ICurrentUser _user;

    public TourScheduleService(ITourScheduleRepository repository, ICurrentUser user)
    {
        _repository = repository;
        _user = user;
    }

    public async Task<TourScheduleResponse> CreateAsync(CreateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        await ValidateBusinessRulesAsync(request, excludeScheduleId: null, cancellationToken);

        var schedule = new tour_schedule
        {
            id = Guid.NewGuid(),
            tour_id = request.tour_id,
            boat_id = request.boat_id,
            dock_id = request.dock_id,
            start_time = request.start_time,
            end_time = request.end_time,
            status = request.status,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _repository.AddAsync(schedule, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(schedule);
    }

    public async Task<TourScheduleResponse> UpdateAsync(Guid id, UpdateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.ScheduleNotFound, ErrorCode.Messages.ScheduleNotFound);

        await EnsureCanMutateAsync(schedule.boat_id, schedule.tour_id, cancellationToken);
        await ValidateBusinessRulesAsync(request, excludeScheduleId: id, cancellationToken);

        schedule.tour_id = request.tour_id;
        schedule.boat_id = request.boat_id;
        schedule.dock_id = request.dock_id;
        schedule.start_time = request.start_time;
        schedule.end_time = request.end_time;
        schedule.status = request.status;
        schedule.updated_at = DateTime.UtcNow;

        _repository.Update(schedule);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(schedule);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.ScheduleNotFound, ErrorCode.Messages.ScheduleNotFound);

        await EnsureCanMutateAsync(schedule.boat_id, schedule.tour_id, cancellationToken);

        _repository.Delete(schedule);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TourScheduleResponse>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        var schedules = await _repository.GetByTourIdAsync(tourId, cancellationToken);
        return schedules.Select(Map).ToList();
    }

    private async Task ValidateBusinessRulesAsync(
        CreateTourScheduleRequest request, Guid? excludeScheduleId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsTourAsync(request.tour_id, cancellationToken))
        {
            throw new AppException(ErrorCode.TourNotExists, ErrorCode.Messages.TourNotExists);
        }

        if (request.boat_id.HasValue && !await _repository.ExistsBoatAsync(request.boat_id.Value, cancellationToken))
        {
            throw new AppException(ErrorCode.BoatNotExists, ErrorCode.Messages.BoatNotExists);
        }

        if (request.dock_id.HasValue && !await _repository.ExistsDockAsync(request.dock_id.Value, cancellationToken))
        {
            throw new AppException(ErrorCode.DockNotExists, ErrorCode.Messages.DockNotExists);
        }

        await EnsureCanMutateAsync(request.boat_id, request.tour_id, cancellationToken);

        var isCancelled = string.Equals(
            request.status, TourScheduleStatuses.Cancelled, StringComparison.OrdinalIgnoreCase);
        if (!isCancelled
            && request.boat_id.HasValue
            && await _repository.HasBoatScheduleOverlapAsync(
                request.boat_id.Value, request.start_time, request.end_time, excludeScheduleId, cancellationToken))
        {
            throw new AppException(ErrorCode.ScheduleBoatOverlap, ErrorCode.Messages.ScheduleBoatOverlap);
        }
    }

    /// <summary>
    /// Chỉ chủ thuyền của tàu (hoặc người tạo tour khi chưa gắn tàu) mới được ghi.
    /// Admin được phép sửa để xử lý sự cố, không đi đường tạo lịch hộ owner.
    /// </summary>
    private async Task EnsureCanMutateAsync(Guid? boatId, Guid tourId, CancellationToken cancellationToken)
    {
        if (_user.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (boatId.HasValue)
        {
            var ownerId = await _repository.GetBoatOwnerIdAsync(boatId.Value, cancellationToken);
            if (ownerId != _user.Id)
            {
                throw new AppException(
                    ErrorCode.Forbidden,
                    "Thuyền không tồn tại hoặc không thuộc quyền quản lý của bạn.");
            }

            return;
        }

        var createdBy = await _repository.GetTourCreatedByAsync(tourId, cancellationToken);
        if (createdBy != _user.Id)
        {
            throw new AppException(
                ErrorCode.Forbidden,
                "Tour này không thuộc quyền quản lý của bạn.");
        }
    }

    private static TourScheduleResponse Map(tour_schedule source)
    {
        return new TourScheduleResponse
        {
            id = source.id,
            tour_id = source.tour_id,
            boat_id = source.boat_id,
            boatName = source.boat?.name,
            boatImageUrls = source.boat?.boat_images
                .OrderBy(image => image.sort_order)
                .Select(image => image.image_url)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList() ?? new List<string>(),
            dock_id = source.dock_id,
            start_time = source.start_time,
            end_time = source.end_time,
            status = source.status
        };
    }
}
