using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface ITourRepository
{
    Task<tour?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<tour>> GetListAsync(string? status, string? location, CancellationToken cancellationToken);
    Task AddAsync(tour tour, CancellationToken cancellationToken);
    void Update(tour tour);
    void Delete(tour tour);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
