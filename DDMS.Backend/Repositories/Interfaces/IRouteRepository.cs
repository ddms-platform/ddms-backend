using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IRouteRepository
{
    Task<route?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<route>> GetByTourIdAsync(Guid tourId, CancellationToken cancellationToken);
    Task AddAsync(route route, CancellationToken cancellationToken);
    void Update(route route);
    void Delete(route route);
    Task<bool> ExistsTourAsync(Guid tourId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
