using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Route;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class RouteService : IRouteService
{
    private readonly IRouteRepository _repository;

    public RouteService(IRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<RouteResponse> CreateAsync(CreateRouteRequest request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var route = new route
        {
            id = Guid.NewGuid(),
            tour_id = request.tour_id,
            name = request.name,
            start_point = request.start_point,
            end_point = request.end_point,
            description = request.description,
            sort_order = request.sort_order,
            created_at = DateTime.UtcNow
        };

        await _repository.AddAsync(route, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(route);
    }

    public async Task<RouteResponse> UpdateAsync(Guid id, UpdateRouteRequest request, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);
        var route = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Route not found");

        route.tour_id = request.tour_id;
        route.name = request.name;
        route.start_point = request.start_point;
        route.end_point = request.end_point;
        route.description = request.description;
        route.sort_order = request.sort_order;

        _repository.Update(route);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(route);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var route = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Route not found");

        _repository.Delete(route);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<RouteResponse>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        var routes = await _repository.GetByTourIdAsync(tourId, cancellationToken);
        return routes.Select(Map).ToList();
    }

    private async Task ValidateRequestAsync(CreateRouteRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsTourAsync(request.tour_id, cancellationToken))
        {
            throw new BadRequestException("Tour does not exist");
        }

        if (string.IsNullOrWhiteSpace(request.start_point) || string.IsNullOrWhiteSpace(request.end_point))
        {
            throw new BadRequestException("Start point and end point are required");
        }
    }

    private static RouteResponse Map(route source)
    {
        return new RouteResponse
        {
            id = source.id,
            tour_id = source.tour_id,
            name = source.name,
            start_point = source.start_point,
            end_point = source.end_point,
            description = source.description,
            sort_order = source.sort_order
        };
    }
}
