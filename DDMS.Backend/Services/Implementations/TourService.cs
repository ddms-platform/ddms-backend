using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourService : ITourService
{
    private static readonly HashSet<string> ValidStatus = ["active", "inactive"];
    private readonly ITourRepository _tourRepository;

    public TourService(ITourRepository tourRepository)
    {
        _tourRepository = tourRepository;
    }

    public async Task<TourResponse> CreateAsync(CreateTourRequest request, CancellationToken cancellationToken)
    {
        var newTour = new tour
        {
            id = Guid.NewGuid(),
            name = request.name.Trim(),
            price = request.price,
            description = request.description,
            duration_minutes = request.duration_minutes,
            location = request.location,
            status = request.status,
            cancel_policy = request.cancel_policy,
            cancel_hours = request.cancel_hours,
            avg_rating = 0,
            total_reviews = 0,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _tourRepository.AddAsync(newTour, cancellationToken);
        await _tourRepository.SaveChangesAsync(cancellationToken);
        return MapTour(newTour);
    }

    public async Task<TourResponse> UpdateAsync(Guid id, UpdateTourRequest request, CancellationToken cancellationToken)
    {
        var currentTour = await _tourRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.TourNotFound, ErrorCode.Messages.TourNotFound);

        currentTour.name = request.name.Trim();
        currentTour.price = request.price;
        currentTour.description = request.description;
        currentTour.duration_minutes = request.duration_minutes;
        currentTour.location = request.location;
        currentTour.status = request.status;
        currentTour.cancel_policy = request.cancel_policy;
        currentTour.cancel_hours = request.cancel_hours;
        currentTour.updated_at = DateTime.UtcNow;

        _tourRepository.Update(currentTour);
        await _tourRepository.SaveChangesAsync(cancellationToken);
        return MapTour(currentTour);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentTour = await _tourRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.TourNotFound, ErrorCode.Messages.TourNotFound);

        _tourRepository.Delete(currentTour);
        await _tourRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<TourResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentTour = await _tourRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.TourNotFound, ErrorCode.Messages.TourNotFound);

        return MapTour(currentTour);
    }

    public async Task<List<TourResponse>> GetListAsync(TourFilterRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.status) && !ValidStatus.Contains(request.status))
        {
            throw new AppException(ErrorCode.TourInvalidStatus, ErrorCode.Messages.TourInvalidStatus);
        }

        var tours = await _tourRepository.GetListAsync(request.status, request.location, cancellationToken);
        return tours.Select(MapTour).ToList();
    }

    private static TourResponse MapTour(tour source)
    {
        return new TourResponse
        {
            id = source.id,
            name = source.name,
            description = source.description,
            price = source.price,
            duration_minutes = source.duration_minutes,
            location = source.location,
            avg_rating = source.avg_rating,
            total_reviews = source.total_reviews,
            status = source.status,
            cancel_policy = source.cancel_policy,
            cancel_hours = source.cancel_hours
        };
    }
}
