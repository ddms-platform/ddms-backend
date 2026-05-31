using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IDockRepository
{
    Task<(List<dock> items, int total)> GetPagedAsync(DockListQuery query);
    Task<List<dock>> GetAllAsync();
    Task<dock?> GetByIdAsync(Guid id);
    Task<DockStatsResponse> GetStatsAsync();
    Task<dock> CreateAsync(dock entity);
    Task UpdateAsync(dock entity);
    Task DeleteAsync(dock entity);
}
