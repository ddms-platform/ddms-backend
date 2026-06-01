using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IRouteService
{
    Task<PagedResponse<RouteItemResponse>> GetRoutesAsync(Guid userId, RouteListQuery query);
    Task<RouteItemResponse> GetByIdAsync(Guid id, Guid userId);
    Task<RouteItemResponse> CreateAsync(Guid userId, CreateRouteRequest request);
    Task<RouteItemResponse> UpdateAsync(Guid id, Guid userId, UpdateRouteRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}
