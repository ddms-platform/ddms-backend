using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// Equivalence Partitioning cho AuthService.LogoutAsync — xem TestData/Auth/AuthService/Logout.json.
/// Lớp tương đương: token trống (no-op), token không tồn tại (no-op), token đã bị revoke trước đó (no-op),
/// và token hợp lệ (revoke thành công).
/// </summary>
public class LogoutTests
{
    public record LogoutTestCase(
        string CaseName,
        string RefreshToken,
        bool TokenExists,
        bool AlreadyRevoked,
        bool ExpectRevokeCalled);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<LogoutTestCase>("TestData/Auth/AuthService/Logout.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task LogoutAsync_EquivalencePartitions(LogoutTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var existingToken = c.TokenExists
            ? new RefreshTokenBuilder().Build()
            : null;
        if (existingToken is not null) existingToken.revoked = c.AlreadyRevoked;

        refreshTokenRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>())).ReturnsAsync(existingToken);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        await service.LogoutAsync(new LogoutRequest { refreshToken = c.RefreshToken });

        refreshTokenRepo.Verify(
            r => r.RevokeAsync(It.IsAny<Models.Entities.refresh_token>()),
            c.ExpectRevokeCalled ? Times.Once() : Times.Never());
    }
}
