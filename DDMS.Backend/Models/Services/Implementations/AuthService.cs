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
            throw new AppException(ErrorCodes.AuthEmailAlreadyExists, MessageConstants.EmailAlreadyExists);
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
            message = MessageConstants.CheckEmailForVerification,
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
            throw new AppException(ErrorCodes.AuthInvalidCredentials, MessageConstants.InvalidCredentials);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.password, user.password_hash))
        {
            throw new AppException(ErrorCodes.AuthInvalidCredentials, MessageConstants.InvalidCredentials);
        }

        _authSessionService.EnsureAccountActive(user);
        _authSessionService.EnsureEmailVerified(user);

        return await _authSessionService.IssueTokensAsync(user, ipAddress, userAgent);
    }

    public async Task<AuthTokensResponse> VerifyEmailAsync(
        VerifyEmailRequest request,
        string? ipAddress,
        string? userAgent)
    {
        var email = await _emailVerificationService.VerifyByTokenAsync(request.token);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            throw new AppException(ErrorCodes.AuthInvalidCredentials, MessageConstants.InvalidCredentials);
        }

        _authSessionService.EnsureAccountActive(user);
        return await _authSessionService.IssueTokensAsync(user, ipAddress, userAgent);
    }

    public async Task<MessageResponse> ResendVerificationEmailAsync(ResendVerificationEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.email))
        {
            throw new ValidationException(MessageConstants.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["email"] = [MessageConstants.EmailRequired]
            });
        }

        var email = request.email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            return new MessageResponse { message = MessageConstants.VerificationLinkSent };
        }

        if (user.email_verified_at is not null)
        {
            throw new AppException(ErrorCodes.AuthValidationFailed, MessageConstants.EmailAlreadyVerified);
        }

        var link = await _emailVerificationService.SendVerificationLinkAsync(email);

        return new MessageResponse
        {
            message = MessageConstants.VerificationLinkSent,
            verificationLink = _hostEnvironment.IsDevelopment() ? link : null
        };
    }

    public async Task<AuthTokensResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.refreshToken))
        {
            throw new AppException(ErrorCodes.AuthRefreshTokenInvalid, MessageConstants.RefreshTokenInvalid);
        }

        var tokenHash = _tokenService.HashToken(request.refreshToken.Trim());
        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (existingToken is null)
        {
            throw new AppException(ErrorCodes.AuthRefreshTokenInvalid, MessageConstants.RefreshTokenInvalid);
        }

        if (existingToken.revoked)
        {
            throw new AppException(ErrorCodes.AuthRefreshTokenRevoked, MessageConstants.RefreshTokenRevoked);
        }

        if (existingToken.expires_at <= DateTime.UtcNow)
        {
            throw new AppException(ErrorCodes.AuthRefreshTokenExpired, MessageConstants.RefreshTokenExpired);
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

    public async Task<CurrentUserResponse> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);

        if (user is null)
        {
            throw new UnauthorizedException();
        }

        _authSessionService.EnsureAccountActive(user);

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
            fieldErrors["fullName"] = [MessageConstants.FullNameRequired];
        }

        if (string.IsNullOrWhiteSpace(request.email))
        {
            fieldErrors["email"] = [MessageConstants.EmailRequired];
        }

        if (string.IsNullOrWhiteSpace(request.password))
        {
            fieldErrors["password"] = [MessageConstants.PasswordRequired];
        }
        else if (request.password.Length < 8)
        {
            fieldErrors["password"] = [MessageConstants.PasswordMinLength];
        }

        if (request.password != request.confirmPassword)
        {
            fieldErrors["confirmPassword"] = [MessageConstants.ConfirmPasswordMismatch];
        }

        if (fieldErrors.Count > 0)
        {
            throw new ValidationException(MessageConstants.ValidationFailed, fieldErrors);
        }
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        var fieldErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.email))
        {
            fieldErrors["email"] = [MessageConstants.EmailRequired];
        }

        if (string.IsNullOrWhiteSpace(request.password))
        {
            fieldErrors["password"] = [MessageConstants.PasswordRequired];
        }

        if (fieldErrors.Count > 0)
        {
            throw new ValidationException(MessageConstants.ValidationFailed, fieldErrors);
        }
    }
}
