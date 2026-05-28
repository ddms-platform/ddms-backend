using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Repositories.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<email_verification_token?> GetByTokenHashAsync(string tokenHash);
    Task<email_verification_token?> GetLatestCreatedAsync(string email, string purpose);
    Task<int> CountRecentRequestsAsync(string email, string purpose, DateTime sinceUtc);
    Task<email_verification_token> AddAsync(email_verification_token entity);
    Task MarkUsedAsync(email_verification_token entity);
    Task InvalidateActiveAsync(string email, string purpose);
}
