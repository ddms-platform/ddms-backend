using System.Security.Cryptography;
using System.Text;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Models.Services.Implementations;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly EmailVerificationOptions _options;

    public EmailVerificationService(
        IEmailVerificationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IEmailSender emailSender,
        IOptions<EmailVerificationOptions> options)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public async Task<string> SendVerificationLinkAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        await EnsureCanSendAsync(normalizedEmail, VerificationPurposes.EmailVerification);

        var rawToken = GenerateSecureToken();
        var entity = new email_verification_token
        {
            id = Guid.NewGuid(),
            email = normalizedEmail,
            token_hash = HashToken(rawToken),
            purpose = VerificationPurposes.EmailVerification,
            expires_at = DateTime.UtcNow.AddMinutes(_options.tokenExpiryMinutes),
            created_at = DateTime.UtcNow
        };

        await _tokenRepository.InvalidateActiveAsync(normalizedEmail, VerificationPurposes.EmailVerification);
        await _tokenRepository.AddAsync(entity);

        var link = BuildVerificationLink(rawToken);
        await _emailSender.SendVerificationLinkEmailAsync(normalizedEmail, link, _options.tokenExpiryMinutes);

        return link;
    }

    public async Task<EmailVerificationResult> VerifyByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ValidationException(ErrorCode.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["token"] = [ErrorCode.Messages.VerificationTokenRequired]
            });
        }

        var record = await _tokenRepository.GetByTokenHashAnyAsync(HashToken(token.Trim()));

        if (record is null)
        {
            throw new AppException(ErrorCode.AuthOtpInvalid, ErrorCode.Messages.VerificationTokenInvalid);
        }

        var user = await _userRepository.GetByEmailAsync(record.email);

        // Idempotent: a previously verified account re-clicking the link is a
        // success, not an error.
        if (user is not null && user.email_verified_at is not null)
        {
            return new EmailVerificationResult(record.email, alreadyVerified: true);
        }

        // Beyond this point the token must still be usable (unused & unexpired).
        if (record.used_at is not null || record.expires_at <= DateTime.UtcNow)
        {
            throw new AppException(ErrorCode.AuthOtpExpired, ErrorCode.Messages.VerificationTokenExpired);
        }

        await _tokenRepository.MarkUsedAsync(record);

        if (user is not null)
        {
            await _userRepository.MarkEmailVerifiedAsync(user.id);
        }

        return new EmailVerificationResult(record.email, alreadyVerified: false);
    }

    private string BuildVerificationLink(string rawToken)
    {
        var baseUrl = _options.frontendBaseUrl.TrimEnd('/');
        var path = _options.verifyEmailPath.StartsWith('/') ? _options.verifyEmailPath : $"/{_options.verifyEmailPath}";
        return $"{baseUrl}{path}?token={Uri.EscapeDataString(rawToken)}";
    }

    private async Task EnsureCanSendAsync(string email, string purpose)
    {
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
