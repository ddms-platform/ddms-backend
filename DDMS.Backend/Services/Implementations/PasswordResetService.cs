using System.Security.Cryptography;
using System.Text;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

public class PasswordResetService : IPasswordResetService
{
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly EmailVerificationOptions _options;

    public PasswordResetService(
        IEmailVerificationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailSender emailSender,
        IOptions<EmailVerificationOptions> options)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public async Task<string?> SendResetLinkAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null
            || user.is_active is false
            || string.IsNullOrWhiteSpace(user.password_hash))
        {
            return null;
        }

        await EnsureCanSendAsync(normalizedEmail);

        var rawToken = GenerateSecureToken();
        var entity = new email_verification_token
        {
            id = Guid.NewGuid(),
            email = normalizedEmail,
            token_hash = HashToken(rawToken),
            purpose = VerificationPurposes.PasswordReset,
            expires_at = DateTime.UtcNow.AddMinutes(_options.tokenExpiryMinutes),
            created_at = DateTime.UtcNow
        };

        await _tokenRepository.InvalidateActiveAsync(normalizedEmail, VerificationPurposes.PasswordReset);
        await _tokenRepository.AddAsync(entity);

        var link = BuildResetLink(rawToken);
        await _emailSender.SendPasswordResetEmailAsync(normalizedEmail, link, _options.tokenExpiryMinutes);

        return link;
    }

    public async Task ResetPasswordByTokenAsync(string token, string password, string confirmPassword)
    {
        ValidateResetRequest(token, password, confirmPassword);

        var record = await _tokenRepository.GetByTokenHashAnyAsync(HashToken(token.Trim()));

        if (record is null || record.purpose != VerificationPurposes.PasswordReset)
        {
            throw new AppException(ErrorCode.AuthOtpInvalid, ErrorCode.Messages.PasswordResetTokenInvalid);
        }

        if (record.used_at is not null || record.expires_at <= DateTime.UtcNow)
        {
            throw new AppException(ErrorCode.AuthOtpExpired, ErrorCode.Messages.PasswordResetTokenInvalid);
        }

        var user = await _userRepository.GetByEmailAsync(record.email);

        if (user is null)
        {
            throw new AppException(ErrorCode.AuthOtpInvalid, ErrorCode.Messages.PasswordResetTokenInvalid);
        }

        if (user.is_active is false)
        {
            throw new AppException(ErrorCode.AuthAccountInactive, ErrorCode.Messages.AccountInactive);
        }

        if (string.IsNullOrWhiteSpace(user.password_hash))
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.PasswordResetSocialAccount);
        }

        await _tokenRepository.MarkUsedAsync(record);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        await _userRepository.UpdatePasswordHashAsync(user.id, passwordHash);
        await _refreshTokenRepository.RevokeAllActiveForUserAsync(user.id);
    }

    private string BuildResetLink(string rawToken)
    {
        var baseUrl = _options.frontendBaseUrl.TrimEnd('/');
        var path = _options.resetPasswordPath.StartsWith('/')
            ? _options.resetPasswordPath
            : $"/{_options.resetPasswordPath}";
        return $"{baseUrl}{path}?token={Uri.EscapeDataString(rawToken)}";
    }

    private async Task EnsureCanSendAsync(string email)
    {
        var purpose = VerificationPurposes.PasswordReset;
        var since = DateTime.UtcNow.AddHours(-1);
        var count = await _tokenRepository.CountRecentRequestsAsync(email, purpose, since);
        if (count >= _options.maxRequestsPerHour)
        {
            throw new AppException(ErrorCode.AuthOtpRateLimited, ErrorCode.Messages.VerificationRateLimited);
        }

        var latest = await _tokenRepository.GetLatestCreatedAsync(email, purpose);
        if (latest is not null)
        {
            var cooldownEnds = latest.created_at.AddSeconds(_options.resendCooldownSeconds);
            if (DateTime.UtcNow < cooldownEnds)
            {
                throw new AppException(ErrorCode.AuthOtpRateLimited, ErrorCode.Messages.VerificationRateLimited);
            }
        }
    }

    private static void ValidateResetRequest(string token, string password, string confirmPassword)
    {
        var fieldErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(token))
        {
            fieldErrors["token"] = [ErrorCode.Messages.PasswordResetTokenRequired];
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            fieldErrors["password"] = [ErrorCode.Messages.PasswordRequired];
        }
        else if (!AuthService.IsPasswordPolicyValid(password))
        {
            fieldErrors["password"] = [ErrorCode.Messages.PasswordPolicy];
        }

        if (password != confirmPassword)
        {
            fieldErrors["confirmPassword"] = [ErrorCode.Messages.ConfirmPasswordMismatch];
        }

        if (fieldErrors.Count > 0)
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, fieldErrors);
        }
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
