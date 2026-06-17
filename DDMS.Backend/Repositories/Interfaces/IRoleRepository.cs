using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<List<role>> GetAllAsync();
    Task<List<role>> GetByNamesAsync(IEnumerable<string> names);
}
