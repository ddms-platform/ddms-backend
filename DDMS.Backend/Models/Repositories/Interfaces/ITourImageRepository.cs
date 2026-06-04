using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface ITourImageRepository
{
    Task<List<tour_image>> GetByTourIdAsync(Guid tourId);
    Task<tour_image?> GetByIdAsync(Guid id, Guid tourId);
    Task AddAsync(tour_image entity);
    Task UpdateAsync(tour_image entity);
    Task DeleteAsync(tour_image entity);
}
