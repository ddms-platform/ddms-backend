using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IRouteRepository
{
    Task<(List<route> items, int total)> GetPagedAsync(Guid userId, RouteListQuery query);
    Task<route?> GetByIdAsync(Guid id, Guid userId);
    Task AddAsync(route entity);
    Task UpdateAsync(route entity);
    Task DeleteAsync(route entity);
}
