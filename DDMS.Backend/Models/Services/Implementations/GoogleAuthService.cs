using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Models.Services.Implementations;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthSessionService _authSessionService;
    private readonly GoogleOptions _googleOptions;

    public GoogleAuthService(
        IUserRepository userRepository,
        IAuthSessionService authSessionService,
        IOptions<GoogleOptions> googleOptions)
    {
        _userRepository = userRepository;
        _authSessionService = authSessionService;
        _googleOptions = googleOptions.Value;
    }

    public async Task<AuthTokensResponse> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.idToken))
        {
            throw new ValidationException(ErrorDefinitions.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["idToken"] = ["Google idToken is required"]
            });
        }

        if (string.IsNullOrWhiteSpace(_googleOptions.clientId))
        {
            throw new AppException(ErrorDefinitions.Codes.AuthGoogleTokenInvalid, ErrorDefinitions.Messages.GoogleTokenInvalid);
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.idToken.Trim(),
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_googleOptions.clientId]
                });
        }
        catch
        {
            throw new AppException(ErrorDefinitions.Codes.AuthGoogleTokenInvalid, ErrorDefinitions.Messages.GoogleTokenInvalid);
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new AppException(ErrorDefinitions.Codes.AuthGoogleTokenInvalid, ErrorDefinitions.Messages.GoogleTokenInvalid);
        }

        var email = payload.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByGoogleIdAsync(payload.Subject)
            ?? await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            user = new user
            {
                id = Guid.NewGuid(),
                full_name = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name.Trim(),
                email = email,
                google_id = payload.Subject,
                avatar_url = payload.Picture,
                email_verified_at = DateTime.UtcNow,
                is_active = true,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user);
            await _userRepository.AssignRoleAsync(user.id, "user");
        }
        else
        {
            var updated = false;

            if (string.IsNullOrWhiteSpace(user.google_id))
            {
                user.google_id = payload.Subject;
                updated = true;
            }
            else if (user.google_id != payload.Subject)
            {
                throw new AppException(ErrorDefinitions.Codes.AuthGoogleTokenInvalid, ErrorDefinitions.Messages.GoogleTokenInvalid);
            }

            if (user.email_verified_at is null)
            {
                user.email_verified_at = DateTime.UtcNow;
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(user.avatar_url) && !string.IsNullOrWhiteSpace(payload.Picture))
            {
                user.avatar_url = payload.Picture;
                updated = true;
            }

            if (updated)
            {
                await _userRepository.UpdateAsync(user);
            }
        }

        _authSessionService.EnsureAccountActive(user);
        return await _authSessionService.IssueTokensAsync(user, ipAddress, userAgent);
    }
}
