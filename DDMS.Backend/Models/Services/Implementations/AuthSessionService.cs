using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Models.Services.Implementations;

public class AuthSessionService : IAuthSessionService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthSessionService(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthTokensResponse> IssueTokensAsync(user user, string? ipAddress, string? userAgent)
    {
        var roles = user.user_roles.Select(x => x.role.name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new refresh_token
        {
            id = Guid.NewGuid(),
            user_id = user.id,
            token_hash = _tokenService.HashToken(refreshToken),
            expires_at = DateTime.UtcNow.AddDays(_jwtOptions.refreshTokenDays),
            revoked = false,
            user_agent = userAgent,
            ip_address = ipAddress,
            created_at = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.EnforceSessionLimitAsync(user.id, _jwtOptions.maxRefreshTokensPerUser);

        return new AuthTokensResponse
        {
            token = accessToken,
            accessToken = accessToken,
            refreshToken = refreshToken,
            authenticated = true,
            expiresInSeconds = _jwtOptions.accessTokenMinutes * 60
        };
    }

    public void EnsureEmailVerified(user user)
    {
        if (user.email_verified_at is null)
        {
            throw new AppException(ErrorCode.AuthEmailNotVerified, ErrorCode.Messages.EmailNotVerified);
        }
    }

    public void EnsureAccountActive(user user)
    {
        if (user.is_active is false)
        {
            throw new AppException(ErrorCode.AuthAccountInactive, ErrorCode.Messages.AccountInactive);
        }
    }
}
