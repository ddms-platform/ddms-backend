using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerRoutesService
{
    Task<PagedResponse<RouteItemResponse>> GetRoutesAsync(Guid userId, RouteListQuery query);
    Task<RouteItemResponse> GetByIdAsync(Guid id, Guid userId);
    Task<RouteItemResponse> CreateAsync(Guid userId, CreateRouteRequest request);
    Task<RouteItemResponse> UpdateAsync(Guid id, Guid userId, UpdateRouteRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}
