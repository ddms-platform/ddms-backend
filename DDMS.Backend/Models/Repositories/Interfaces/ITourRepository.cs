using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface ITourRepository
{
    Task<(List<tour> items, int total)> GetPagedAsync(Guid userId, TourListQuery query);
    Task<tour?> GetByIdAsync(Guid id, Guid userId);
    Task AddAsync(tour entity);
    Task UpdateAsync(tour entity);
}
