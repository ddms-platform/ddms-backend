using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// AuthService.LogoutAllAsync chỉ uỷ quyền thẳng xuống IRefreshTokenRepository.RevokeAllActiveForUserAsync
/// (không có nhánh rẽ) — 1 case xác nhận tham số truyền đúng userId.
/// </summary>
public class LogoutAllTests
{
    [Fact]
    public async Task LogoutAllAsync_RevokesAllActiveTokensForUser()
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        await service.LogoutAllAsync(TestGuids.UserId);

        refreshTokenRepo.Verify(r => r.RevokeAllActiveForUserAsync(TestGuids.UserId), Times.Once);
    }
}
