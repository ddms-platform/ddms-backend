using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<refresh_token?> GetByHashAsync(string tokenHash)
    {
        return _dbContext.refresh_tokens
            .Include(x => x.user)
            .ThenInclude(x => x.user_roles)
            .ThenInclude(x => x.role)
            .FirstOrDefaultAsync(x => x.token_hash == tokenHash);
    }

    public async Task<refresh_token> AddAsync(refresh_token entity)
    {
        _dbContext.refresh_tokens.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task RevokeAsync(refresh_token entity)
    {
        entity.revoked = true;
        entity.revoked_at = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}
