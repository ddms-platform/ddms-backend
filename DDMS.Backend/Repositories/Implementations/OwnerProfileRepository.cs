using DDMS.Backend.Data;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class OwnerProfileRepository : IOwnerProfileRepository
{
    private readonly AppDbContext _dbContext;

    public OwnerProfileRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<string?> FindStatusByUserAsync(Guid userId, CancellationToken ct) =>
        _dbContext.owner_profiles
            .AsNoTracking()
            .Where(p => p.user_id == userId)
            .Select(p => p.status)
            .FirstOrDefaultAsync(ct);
}
