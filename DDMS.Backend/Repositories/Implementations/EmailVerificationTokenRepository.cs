using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _dbContext;

    public EmailVerificationTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<email_verification_token?> GetByTokenHashAsync(string tokenHash)
    {
        return _dbContext.email_verification_tokens
            .FirstOrDefaultAsync(x => x.token_hash == tokenHash && x.used_at == null && x.expires_at > DateTime.UtcNow);
    }

    public Task<email_verification_token?> GetByTokenHashAnyAsync(string tokenHash)
    {
        return _dbContext.email_verification_tokens
            .FirstOrDefaultAsync(x => x.token_hash == tokenHash);
    }

    public Task<email_verification_token?> GetLatestCreatedAsync(string email, string purpose)
    {
        return _dbContext.email_verification_tokens
            .Where(x => x.email == email && x.purpose == purpose)
            .OrderByDescending(x => x.created_at)
            .FirstOrDefaultAsync();
    }

    public Task<int> CountRecentRequestsAsync(string email, string purpose, DateTime sinceUtc)
    {
        return _dbContext.email_verification_tokens
            .CountAsync(x => x.email == email && x.purpose == purpose && x.created_at >= sinceUtc);
    }

    public async Task<email_verification_token> AddAsync(email_verification_token entity)
    {
        _dbContext.email_verification_tokens.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task MarkUsedAsync(email_verification_token entity)
    {
        entity.used_at = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task InvalidateActiveAsync(string email, string purpose)
    {
        var now = DateTime.UtcNow;
        var activeTokens = await _dbContext.email_verification_tokens
            .Where(x => x.email == email && x.purpose == purpose && x.used_at == null && x.expires_at > now)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.used_at = now;
        }

        if (activeTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
