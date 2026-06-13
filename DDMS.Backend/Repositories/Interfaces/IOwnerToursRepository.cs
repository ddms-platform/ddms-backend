using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IOwnerToursRepository
{
    Task<(List<tour> items, int total)> GetPagedAsync(Guid userId, TourListQuery query);
    Task<tour?> GetByIdAsync(Guid id, Guid userId);
    Task<tour?> GetActiveByIdAsync(Guid id);
    Task AddAsync(tour entity);
    Task UpdateAsync(tour entity);
}
