using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<refresh_token?> GetByHashAsync(string tokenHash);
    Task<refresh_token> AddAsync(refresh_token entity);
    Task RevokeAsync(refresh_token entity);
    Task RevokeAllActiveForUserAsync(Guid userId);
    Task EnforceSessionLimitAsync(Guid userId, int maxSessions);
}
