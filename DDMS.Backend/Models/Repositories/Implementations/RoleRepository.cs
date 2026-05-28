using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _dbContext;

    public RoleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<role>> GetAllAsync()
    {
        return _dbContext.roles.OrderBy(x => x.name).ToListAsync();
    }

    public Task<List<role>> GetByNamesAsync(IEnumerable<string> names)
    {
        var normalized = names.Select(x => x.Trim().ToLowerInvariant()).ToList();
        return _dbContext.roles
            .Where(x => normalized.Contains(x.name.ToLower()))
            .ToListAsync();
    }
}
