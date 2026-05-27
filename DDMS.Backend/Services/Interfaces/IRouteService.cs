using DDMS.Backend.Models.DTOs.Route;

namespace DDMS.Backend.Services.Interfaces;

public interface IRouteService
{
    Task<RouteResponse> CreateAsync(CreateRouteRequest request, CancellationToken cancellationToken);
    Task<RouteResponse> UpdateAsync(Guid id, UpdateRouteRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<RouteResponse>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken);
}
