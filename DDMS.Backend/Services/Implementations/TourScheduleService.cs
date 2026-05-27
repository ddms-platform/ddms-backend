using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.TourSchedule;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourScheduleService : ITourScheduleService
{
    private static readonly HashSet<string> ValidStatus = ["scheduled", "ongoing", "completed", "cancelled"];
    private readonly ITourScheduleRepository _repository;

    public TourScheduleService(ITourScheduleRepository repository)
    {
        _repository = repository;
    }

    public async Task<TourScheduleResponse> CreateAsync(CreateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

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
        await ValidateRequestAsync(request, cancellationToken);
        var schedule = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Schedule not found");

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
            ?? throw new NotFoundException("Schedule not found");

        _repository.Delete(schedule);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TourScheduleResponse>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        var schedules = await _repository.GetByTourIdAsync(tourId, cancellationToken);
        return schedules.Select(Map).ToList();
    }

    private async Task ValidateRequestAsync(CreateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsTourAsync(request.tour_id, cancellationToken))
        {
            throw new BadRequestException("Tour does not exist");
        }

        if (request.boat_id.HasValue && !await _repository.ExistsBoatAsync(request.boat_id.Value, cancellationToken))
        {
            throw new BadRequestException("Boat does not exist");
        }

        if (request.dock_id.HasValue && !await _repository.ExistsDockAsync(request.dock_id.Value, cancellationToken))
        {
            throw new BadRequestException("Dock does not exist");
        }

        if (request.end_time <= request.start_time)
        {
            throw new BadRequestException("End time must be greater than start time");
        }

        if (!ValidStatus.Contains(request.status))
        {
            throw new BadRequestException("Invalid schedule status");
        }
    }

    private static TourScheduleResponse Map(tour_schedule source)
    {
        return new TourScheduleResponse
        {
            id = source.id,
            tour_id = source.tour_id,
            boat_id = source.boat_id,
            dock_id = source.dock_id,
            start_time = source.start_time,
            end_time = source.end_time,
            status = source.status
        };
    }
}
