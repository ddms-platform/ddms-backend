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
            expires_at = DateTime.UtcNow.AddHours(_options.tokenExpiryHours),
            created_at = DateTime.UtcNow
        };

        await _tokenRepository.InvalidateActiveAsync(normalizedEmail, VerificationPurposes.EmailVerification);
        await _tokenRepository.AddAsync(entity);

        var link = BuildVerificationLink(rawToken);
        await _emailSender.SendVerificationLinkEmailAsync(normalizedEmail, link, _options.tokenExpiryHours);

        return link;
    }

    public async Task<string> VerifyByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ValidationException(ErrorDefinitions.Messages.ValidationFailed, new Dictionary<string, List<string>>
            {
                ["token"] = [ErrorDefinitions.Messages.VerificationTokenRequired]
            });
        }

        var record = await _tokenRepository.GetByTokenHashAsync(HashToken(token.Trim()));

        if (record is null)
        {
            throw new AppException(ErrorDefinitions.Codes.AuthOtpExpired, ErrorDefinitions.Messages.VerificationTokenExpired);
        }

        await _tokenRepository.MarkUsedAsync(record);

        var user = await _userRepository.GetByEmailAsync(record.email);
        if (user is not null)
        {
            await _userRepository.MarkEmailVerifiedAsync(user.id);
        }

        return record.email;
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
            throw new AppException(ErrorDefinitions.Codes.AuthOtpRateLimited, ErrorDefinitions.Messages.VerificationRateLimited);
        }

        var latest = await _tokenRepository.GetLatestCreatedAsync(email, purpose);
        if (latest is not null)
        {
            var cooldownEnds = latest.created_at.AddSeconds(_options.resendCooldownSeconds);
            if (DateTime.UtcNow < cooldownEnds)
            {
                throw new AppException(ErrorDefinitions.Codes.AuthOtpRateLimited, ErrorDefinitions.Messages.VerificationRateLimited);
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
