using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;

namespace DDMS.Backend.Models.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthSessionService _authSessionService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IHostEnvironment _hostEnvironment;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IAuthSessionService authSessionService,
        IEmailVerificationService emailVerificationService,
        IHostEnvironment hostEnvironment)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _authSessionService = authSessionService;
        _emailVerificationService = emailVerificationService;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        ValidateRegisterRequest(request);
        var email = request.email.Trim().ToLowerInvariant();

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new AppException(ErrorCode.AuthEmailAlreadyExists, ErrorCode.Messages.EmailAlreadyExists);
        }

        var user = new user
        {
            id = Guid.NewGuid(),
            full_name = request.fullName.Trim(),
            email = email,
            password_hash = BCrypt.Net.BCrypt.HashPassword(request.password),
            is_active = true,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.AssignRoleAsync(user.id, "user");

        var verificationLink = await _emailVerificationService.SendVerificationLinkAsync(email);

        return new RegisterResponse
        {
            requiresEmailVerification = true,
            email = email,
            message = ErrorCode.Messages.CheckEmailForVerification,
            verificationLink = _hostEnvironment.IsDevelopment() ? verificationLink : null
        };
    }

    public async Task<AuthTokensResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent)
    {
        ValidateLoginRequest(request);
        var email = request.email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || string.IsNullOrWhiteSpace(user.password_hash))
        {
            throw new AppException(ErrorCode.AuthInvalidCredentials, ErrorCode.Messages.InvalidCredentials);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.password, user.password_hash))
        {
            throw new AppException(ErrorCode.AuthInvalidCredentials, ErrorCode.Messages.InvalidCredentials);
        }

        _authSessionService.EnsureAccountActive(user);
        _authSessionService.EnsureEmailVerified(user);

        return await _authSessionService.IssueTokensAsync(user, ipAddress, userAgent);
    }

    public async Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var result = await _emailVerificationService.VerifyByTokenAsync(request.token);
        var user = await _userRepository.GetByEmailAsync(result.email);

        if (user is null)
        {
            throw new AppException(ErrorCode.AuthInvalidCredentials, ErrorCode.Messages.InvalidCredentials);
        }

        _authSessionService.EnsureAccountActive(user);

        return new VerifyEmailResponse
        {
            alreadyVerified = result.alreadyVerified,
            message = result.alreadyVerified
                ? ErrorCode.Messages.EmailAlreadyVerified
                : ErrorCode.Messages.EmailVerified
        };
    }

    public async Task<MessageResponse> ResendVerificationEmailAsync(ResendVerificationEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.email))
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["email"] = [ErrorCode.Messages.EmailRequired]
            });
        }

        var email = request.email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            return new MessageResponse { message = ErrorCode.Messages.VerificationLinkSent };
        }

        if (user.email_verified_at is not null)
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.EmailAlreadyVerified);
        }

        var link = await _emailVerificationService.SendVerificationLinkAsync(email);

        return new MessageResponse
        {
            message = ErrorCode.Messages.VerificationLinkSent,
            verificationLink = _hostEnvironment.IsDevelopment() ? link : null
        };
    }

    public async Task<AuthTokensResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.refreshToken))
        {
            throw new AppException(ErrorCode.AuthRefreshTokenInvalid, ErrorCode.Messages.RefreshTokenInvalid);
        }

        var tokenHash = _tokenService.HashToken(request.refreshToken.Trim());
        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (existingToken is null)
        {
            throw new AppException(ErrorCode.AuthRefreshTokenInvalid, ErrorCode.Messages.RefreshTokenInvalid);
        }

        if (existingToken.revoked)
        {
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(existingToken.user_id);
            throw new AppException(ErrorCode.AuthRefreshTokenReuseDetected, ErrorCode.Messages.RefreshTokenReuseDetected);
        }

        if (existingToken.expires_at <= DateTime.UtcNow)
        {
            throw new AppException(ErrorCode.AuthRefreshTokenExpired, ErrorCode.Messages.RefreshTokenExpired);
        }

        await _refreshTokenRepository.RevokeAsync(existingToken);
        _authSessionService.EnsureAccountActive(existingToken.user);
        _authSessionService.EnsureEmailVerified(existingToken.user);

        return await _authSessionService.IssueTokensAsync(existingToken.user, ipAddress, userAgent);
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.refreshToken))
        {
            return;
        }

        var tokenHash = _tokenService.HashToken(request.refreshToken.Trim());
        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (existingToken is null || existingToken.revoked)
        {
            return;
        }

        await _refreshTokenRepository.RevokeAsync(existingToken);
    }

    public async Task LogoutAllAsync(Guid userId)
    {
        await _refreshTokenRepository.RevokeAllActiveForUserAsync(userId);
    }

    public async Task<CurrentUserResponse> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);

        if (user is null)
        {
            throw new UnauthorizedException();
        }

        _authSessionService.EnsureAccountActive(user);
        _authSessionService.EnsureEmailVerified(user);

        return new CurrentUserResponse
        {
            id = user.id,
            fullName = user.full_name,
            email = user.email,
            roles = user.user_roles.Select(x => x.role.name).ToList(),
            emailVerified = user.email_verified_at is not null
        };
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        var fieldErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.fullName))
        {
            fieldErrors["fullName"] = [ErrorCode.Messages.FullNameRequired];
        }

        if (string.IsNullOrWhiteSpace(request.email))
        {
            fieldErrors["email"] = [ErrorCode.Messages.EmailRequired];
        }

        if (string.IsNullOrWhiteSpace(request.password))
        {
            fieldErrors["password"] = [ErrorCode.Messages.PasswordRequired];
        }
        else if (request.password.Length < 8)
        {
            fieldErrors["password"] = [ErrorCode.Messages.PasswordMinLength];
        }

        if (request.password != request.confirmPassword)
        {
            fieldErrors["confirmPassword"] = [ErrorCode.Messages.ConfirmPasswordMismatch];
        }

        if (fieldErrors.Count > 0)
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, fieldErrors);
        }
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        var fieldErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.email))
        {
            fieldErrors["email"] = [ErrorCode.Messages.EmailRequired];
        }

        if (string.IsNullOrWhiteSpace(request.password))
        {
            fieldErrors["password"] = [ErrorCode.Messages.PasswordRequired];
        }

        if (fieldErrors.Count > 0)
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, fieldErrors);
        }
    }
}
