using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourService : ITourService
{
    private static readonly HashSet<string> ValidStatus = TourConstants.Statuses.Allowed;
    private readonly ITourRepository _tourRepository;
    private readonly INotificationService _notificationService;

    public TourService(
        ITourRepository tourRepository,
        INotificationService notificationService)
    {
        _tourRepository = tourRepository;
        _notificationService = notificationService;
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
            status = NormalizeStatus(request.status),
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

        var oldStatus = currentTour.status;
        var newStatus = NormalizeStatus(request.status);

        currentTour.name = request.name.Trim();
        currentTour.price = request.price;
        currentTour.description = request.description;
        currentTour.duration_minutes = request.duration_minutes;
        currentTour.location = request.location;
        currentTour.status = newStatus;
        currentTour.cancel_policy = request.cancel_policy;
        currentTour.cancel_hours = request.cancel_hours;
        currentTour.updated_at = DateTime.UtcNow;

        _tourRepository.Update(currentTour);
        await _tourRepository.SaveChangesAsync(cancellationToken);

        // Gửi thông báo đến chủ thuyền khi trạng thái thay đổi
        if (currentTour.created_by.HasValue && !string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (string.Equals(newStatus, TourConstants.Statuses.Active, StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.CreateNotificationAsync(
                        senderId: null,
                        type: "system",
                        title: "Tour du lịch đã được phê duyệt 🌟",
                        body: $"Chúc mừng! Tour '{currentTour.name}' của bạn đã được Ban Quản Trị phê duyệt và bắt đầu mở bán trên hệ thống.",
                        recipientIds: new List<Guid> { currentTour.created_by.Value },
                        data: null,
                        ct: cancellationToken
                    );
                }
                else if (string.Equals(newStatus, TourConstants.Statuses.Rejected, StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.CreateNotificationAsync(
                        senderId: null,
                        type: "system",
                        title: "Tour du lịch chưa được duyệt ❌",
                        body: $"Tour '{currentTour.name}' của bạn chưa được phê duyệt. Vui lòng kiểm tra lại thông tin, lịch trình hoặc hình ảnh và gửi lại yêu cầu.",
                        recipientIds: new List<Guid> { currentTour.created_by.Value },
                        data: null,
                        ct: cancellationToken
                    );
                }
            }
            catch { /* best effort */ }
        }

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
        var normalizedStatus = string.IsNullOrWhiteSpace(request.status)
            ? null
            : request.status.Trim().ToLowerInvariant();

        if (normalizedStatus is not null && !ValidStatus.Contains(normalizedStatus))
        {
            throw new AppException(ErrorCode.TourInvalidStatus, ErrorCode.Messages.TourInvalidStatus);
        }

        var tours = await _tourRepository.GetListAsync(normalizedStatus, request.location, cancellationToken);
        return tours.Select(MapTour).ToList();
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToLowerInvariant();
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
