using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// AuthService.ResetPasswordAsync chỉ uỷ quyền xuống IPasswordResetService.ResetPasswordByTokenAsync
/// (không có nhánh rẽ riêng) — 2 case: thành công trả message cố định, và lỗi từ token service được
/// truyền nguyên vẹn lên trên (không bị nuốt).
/// </summary>
public class ResetPasswordTests
{
    private static (Mock<DDMS.Backend.Repositories.Interfaces.IUserRepository> userRepo,
        Mock<DDMS.Backend.Services.Interfaces.IPasswordResetService> passwordResetService,
        DDMS.Backend.Services.Implementations.AuthService service) CreateSut()
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

        return (userRepo, passwordResetService, service);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ReturnsSuccessMessage()
    {
        var (_, passwordResetService, service) = CreateSut();

        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            token = "valid-token",
            password = "Abcdefg1!",
            confirmPassword = "Abcdefg1!"
        });

        result.message.Should().Be(ErrorCode.Messages.PasswordResetSuccess);
        passwordResetService.Verify(s => s.ResetPasswordByTokenAsync("valid-token", "Abcdefg1!", "Abcdefg1!"), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_PropagatesException()
    {
        var (_, passwordResetService, service) = CreateSut();
        passwordResetService.Setup(s => s.ResetPasswordByTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new AppException(ErrorCode.AuthRefreshTokenInvalid, "Token không hợp lệ"));

        var act = async () => await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            token = "invalid-token",
            password = "Abcdefg1!",
            confirmPassword = "Abcdefg1!"
        });

        await act.Should().ThrowAsync<AppException>();
    }
}
