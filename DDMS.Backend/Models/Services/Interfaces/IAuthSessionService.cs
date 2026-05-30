using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IAuthSessionService
{
    Task<AuthTokensResponse> IssueTokensAsync(user user, string? ipAddress, string? userAgent);
    void EnsureEmailVerified(user user);
    void EnsureAccountActive(user user);
}
