using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// Equivalence Partitioning cho AuthService.RefreshTokenAsync — xem TestData/Auth/AuthService/RefreshToken.json.
/// Lớp tương đương: token trống, không tìm thấy, đã bị thu hồi (nghi ngờ tái sử dụng -> thu hồi toàn bộ),
/// đã hết hạn, và làm mới thành công.
/// </summary>
public class RefreshTokenTests
{
    public record RefreshTokenTestCase(
        string CaseName,
        string RefreshToken,
        bool TokenExists,
        bool Revoked,
        int ExpiresInHoursOffset,
        string? ExpectedException);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<RefreshTokenTestCase>("TestData/Auth/AuthService/RefreshToken.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task RefreshTokenAsync_EquivalencePartitions(RefreshTokenTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var user = new UserBuilder().Build();
        var existingToken = c.TokenExists
            ? new RefreshTokenBuilder().WithUser(user)
                .WithExpiresAt(DateTime.UtcNow.AddHours(c.ExpiresInHoursOffset))
                .Build()
            : null;
        if (existingToken is not null) existingToken.revoked = c.Revoked;

        refreshTokenRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>())).ReturnsAsync(existingToken);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var act = async () => await service.RefreshTokenAsync(
            new RefreshTokenRequest { refreshToken = c.RefreshToken }, "127.0.0.1", "test-agent");

        switch (c.ExpectedException)
        {
            case "Invalid":
                var invalidEx = await act.Should().ThrowAsync<AppException>();
                invalidEx.Which.ShouldBeAppException(ErrorCode.AuthRefreshTokenInvalid);
                break;
            case "ReuseDetected":
                var reuseEx = await act.Should().ThrowAsync<AppException>();
                reuseEx.Which.ShouldBeAppException(ErrorCode.AuthRefreshTokenReuseDetected);
                refreshTokenRepo.Verify(r => r.RevokeAllActiveForUserAsync(existingToken!.user_id), Times.Once);
                break;
            case "Expired":
                var expiredEx = await act.Should().ThrowAsync<AppException>();
                expiredEx.Which.ShouldBeAppException(ErrorCode.AuthRefreshTokenExpired);
                break;
            default:
                var result = await act.Should().NotThrowAsync();
                result.Subject.Should().NotBeNull();
                refreshTokenRepo.Verify(r => r.RevokeAsync(existingToken!), Times.Once);
                authSessionService.Verify(s => s.IssueTokensAsync(user, "127.0.0.1", "test-agent"), Times.Once);
                break;
        }
    }
}
