using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class RouteService : IRouteService
{
    private readonly IRouteRepository _routeRepository;
    private readonly ITourRepository _tourRepository;

    public RouteService(IRouteRepository routeRepository, ITourRepository tourRepository)
    {
        _routeRepository = routeRepository;
        _tourRepository = tourRepository;
    }

    public async Task<PagedResponse<RouteItemResponse>> GetRoutesAsync(Guid userId, RouteListQuery query)
    {
        if (query.tourId.HasValue)
        {
            var tourEntity = await _tourRepository.GetByIdAsync(query.tourId.Value, userId);
            if (tourEntity is null)
            {
                throw new AppException(ErrorCode.RouteTourNotFound, ErrorCode.Messages.RouteTourNotFound);
            }
        }

        var (items, total) = await _routeRepository.GetPagedAsync(userId, query);
        var pageSize = query.pageSize is < 1 or > 100 ? 10 : query.pageSize;
        var page = query.page < 1 ? 1 : query.page;

        return new PagedResponse<RouteItemResponse>
        {
            items = items.Select(MapRoute).ToList(),
            page = page,
            pageSize = pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<RouteItemResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var entity = await _routeRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapRoute(entity);
    }

    public async Task<RouteItemResponse> CreateAsync(Guid userId, CreateRouteRequest request)
    {
        ValidateRouteInput(request.startPoint, request.endPoint, request.sortOrder);

        var tourEntity = await _tourRepository.GetByIdAsync(request.tourId, userId);
        if (tourEntity is null)
        {
            throw new AppException(ErrorCode.RouteTourNotFound, ErrorCode.Messages.RouteTourNotFound);
        }

        var entity = new route
        {
            id = Guid.NewGuid(),
            tour_id = request.tourId,
            name = NormalizeOptional(request.name),
            start_point = request.startPoint.Trim(),
            end_point = request.endPoint.Trim(),
            description = NormalizeOptional(request.description),
            sort_order = request.sortOrder,
            created_at = DateTime.UtcNow
        };

        await _routeRepository.AddAsync(entity);
        var created = await _routeRepository.GetByIdAsync(entity.id, userId);
        return MapRoute(created!);
    }

    public async Task<RouteItemResponse> UpdateAsync(Guid id, Guid userId, UpdateRouteRequest request)
    {
        ValidateRouteInput(request.startPoint, request.endPoint, request.sortOrder);

        var tourEntity = await _tourRepository.GetByIdAsync(request.tourId, userId);
        if (tourEntity is null)
        {
            throw new AppException(ErrorCode.RouteTourNotFound, ErrorCode.Messages.RouteTourNotFound);
        }

        var entity = await _routeRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.tour_id = request.tourId;
        entity.name = NormalizeOptional(request.name);
        entity.start_point = request.startPoint.Trim();
        entity.end_point = request.endPoint.Trim();
        entity.description = NormalizeOptional(request.description);
        entity.sort_order = request.sortOrder;

        await _routeRepository.UpdateAsync(entity);
        var updated = await _routeRepository.GetByIdAsync(id, userId);
        return MapRoute(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var entity = await _routeRepository.GetByIdAsync(id, userId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        await _routeRepository.DeleteAsync(entity);
    }

    private static void ValidateRouteInput(string startPoint, string endPoint, int sortOrder)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(startPoint))
        {
            errors["startPoint"] = [ErrorCode.Messages.RouteStartPointRequired];
        }

        if (string.IsNullOrWhiteSpace(endPoint))
        {
            errors["endPoint"] = [ErrorCode.Messages.RouteEndPointRequired];
        }

        if (sortOrder < 0)
        {
            errors["sortOrder"] = [ErrorCode.Messages.RouteSortOrderInvalid];
        }

        if (errors.Count > 0)
        {
            throw new AppException(ErrorCode.RouteValidationFailed, ErrorCode.Messages.TourValidationFailed, errors);
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static RouteItemResponse MapRoute(route entity)
    {
        return new RouteItemResponse
        {
            id = entity.id,
            tourId = entity.tour_id,
            tourName = entity.tour.name,
            name = entity.name,
            startPoint = entity.start_point,
            endPoint = entity.end_point,
            description = entity.description,
            sortOrder = entity.sort_order,
            createdAt = entity.created_at
        };
    }
}
