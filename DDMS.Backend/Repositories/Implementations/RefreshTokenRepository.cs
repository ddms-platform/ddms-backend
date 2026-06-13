using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

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

    public async Task RevokeAllActiveForUserAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var tokens = await _dbContext.refresh_tokens
            .Where(x => x.user_id == userId && !x.revoked && x.expires_at > now)
            .ToListAsync();

        if (tokens.Count == 0)
        {
            return;
        }

        foreach (var token in tokens)
        {
            token.revoked = true;
            token.revoked_at = now;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task EnforceSessionLimitAsync(Guid userId, int maxSessions)
    {
        if (maxSessions < 1)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var activeTokens = await _dbContext.refresh_tokens
            .Where(x => x.user_id == userId && !x.revoked && x.expires_at > now)
            .OrderByDescending(x => x.created_at)
            .ToListAsync();

        if (activeTokens.Count <= maxSessions)
        {
            return;
        }

        foreach (var token in activeTokens.Skip(maxSessions))
        {
            token.revoked = true;
            token.revoked_at = now;
        }

        await _dbContext.SaveChangesAsync();
    }
}
