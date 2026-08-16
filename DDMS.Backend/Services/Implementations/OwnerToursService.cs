using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class OwnerToursService : IOwnerToursService
{
    private readonly IOwnerToursRepository _tourRepository;
    private readonly IOwnerDocumentService _ownerDocs;

    public OwnerToursService(
        IOwnerToursRepository tourRepository,
        IOwnerDocumentService ownerDocs)
    {
        _tourRepository = tourRepository;
        _ownerDocs = ownerDocs;
    }

    public async Task<PagedResponse<TourItemResponse>> GetToursAsync(Guid userId, TourListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var normalizedStatus = query.status.Trim().ToLowerInvariant();
            if (!TourConstants.Statuses.Allowed.Contains(normalizedStatus))
            {
                throw new AppException(ErrorCode.TourStatusInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
                {
                    ["status"] = [ErrorCode.Messages.TourStatusInvalid]
                });
            }
        }

        var (items, total) = await _tourRepository.GetPagedAsync(userId, query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        return new PagedResponse<TourItemResponse>
        {
            items = items.Select(MapTour).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<TourItemResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var entity = await _tourRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapTour(entity);
    }

    public async Task<TourItemResponse> CreateAsync(Guid userId, CreateTourRequest request)
    {
        await EnsureOwnerCanManageToursAsync(userId);
        ValidateCoreFields(request.name, request.price, request.durationMinutes);
        var (cancelPolicy, cancelHours) = ValidateCancelPolicy(request.cancelPolicy, request.cancelHours);

        var entity = new tour
        {
            id = Guid.NewGuid(),
            name = request.name.Trim(),
            price = request.price,
            description = NormalizeOptional(request.description),
            duration_minutes = request.durationMinutes,
            location = NormalizeOptional(request.location),
            status = TourConstants.Statuses.Pending,
            cancel_policy = cancelPolicy,
            cancel_hours = cancelHours,
            created_by = userId,
            avg_rating = 0,
            total_reviews = 0,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _tourRepository.AddAsync(entity);
        return MapTour(entity);
    }

    public async Task<TourItemResponse> UpdateAsync(Guid id, Guid userId, UpdateTourRequest request)
    {
        await EnsureOwnerCanManageToursAsync(userId);
        ValidateCoreFields(request.name, request.price, request.durationMinutes);

        var normalizedStatus = request.status.Trim().ToLowerInvariant();
        if (!TourConstants.Statuses.Allowed.Contains(normalizedStatus))
        {
            throw new AppException(ErrorCode.TourStatusInvalid, ErrorCode.Messages.TourValidationFailed, new Dictionary<string, List<string>>
            {
                ["status"] = [ErrorCode.Messages.TourStatusInvalid]
            });
        }

        var (cancelPolicy, cancelHours) = ValidateCancelPolicy(request.cancelPolicy, request.cancelHours);

        var entity = await _tourRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.name = request.name.Trim();
        entity.price = request.price;
        entity.description = NormalizeOptional(request.description);
        entity.duration_minutes = request.durationMinutes;
        entity.location = NormalizeOptional(request.location);
        entity.status = normalizedStatus;
        entity.cancel_policy = cancelPolicy;
        entity.cancel_hours = cancelHours;

        await _tourRepository.UpdateAsync(entity);
        return MapTour(entity);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await EnsureOwnerCanManageToursAsync(userId);
        var entity = await _tourRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        // Soft delete to avoid breaking booking/schedule relations.
        entity.status = TourConstants.Statuses.Inactive;
        await _tourRepository.UpdateAsync(entity);
    }

    private static void ValidateCoreFields(string name, decimal price, int durationMinutes)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = [ErrorCode.Messages.TourNameRequired];
        }

        if (price < 0)
        {
            errors["price"] = [ErrorCode.Messages.TourPriceInvalid];
        }

        if (durationMinutes <= 0)
        {
            errors["durationMinutes"] = [ErrorCode.Messages.TourDurationInvalid];
        }

        if (errors.Count > 0)
        {
            throw new AppException(ErrorCode.TourValidationFailed, ErrorCode.Messages.TourValidationFailed, errors);
        }
    }

    private static (string cancelPolicy, int? cancelHours) ValidateCancelPolicy(string cancelPolicyInput, int? cancelHoursInput)
    {
        var cancelPolicy = cancelPolicyInput.Trim().ToLowerInvariant();
        var errors = new Dictionary<string, List<string>>();

        if (!TourConstants.CancelPolicies.Allowed.Contains(cancelPolicy))
        {
            errors["cancelPolicy"] = [ErrorCode.Messages.TourCancelPolicyInvalid];
        }

        var cancelHours = cancelHoursInput;
        if (cancelPolicy == TourConstants.CancelPolicies.Free)
        {
            cancelHours = null;
        }
        else if (!cancelHours.HasValue || cancelHours.Value < 0)
        {
            errors["cancelHours"] = [ErrorCode.Messages.TourCancelHoursInvalid];
        }

        if (errors.Count > 0)
        {
            var code = errors.ContainsKey("cancelPolicy")
                ? ErrorCode.TourCancelPolicyInvalid
                : ErrorCode.TourCancelHoursInvalid;
            throw new AppException(code, ErrorCode.Messages.TourValidationFailed, errors);
        }

        return (cancelPolicy, cancelHours);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static TourItemResponse MapTour(tour entity)
    {
        return new TourItemResponse
        {
            id = entity.id,
            name = entity.name,
            price = entity.price,
            description = entity.description,
            durationMinutes = entity.duration_minutes,
            location = entity.location,
            status = entity.status,
            cancelPolicy = entity.cancel_policy,
            cancelHours = entity.cancel_hours,
            avgRating = entity.avg_rating,
            totalReviews = entity.total_reviews,
            createdAt = entity.created_at,
            updatedAt = entity.updated_at
        };
    }

    private async Task EnsureOwnerCanManageToursAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var overview = await _ownerDocs.GetOverviewByUserIdAsync(userId, ct);
            if (overview != null && overview.IsLocked)
            {
                throw new AppException(
                    ErrorCode.OwnerDocumentOverdueBlocked,
                    ErrorCode.Messages.OwnerDocumentOverdueBlocked);
            }
        }
        catch (NotFoundException)
        {
            // Not an owner profile or admin, allow
        }
    }
}
