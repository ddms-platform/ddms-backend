using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// Equivalence Partitioning cho AuthService.UpdateAvatarAsync: user không tồn tại (Unauthorized)
/// và cập nhật avatar thành công (upload qua Cloudinary, lưu avatar_url mới).
/// </summary>
public class UpdateAvatarTests
{
    private static IFormFile CreateFakeFile(string fileName = "avatar.png")
    {
        var mock = new Mock<IFormFile>();
        var content = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(content);
        mock.Setup(f => f.OpenReadStream()).Returns(stream);
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.Length).Returns(content.Length);
        return mock.Object;
    }

    [Fact]
    public async Task UpdateAvatarAsync_UserNotFound_ThrowsUnauthorized()
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        userRepo.Setup(r => r.GetByIdWithRolesAsync(TestGuids.UserId)).ReturnsAsync((Models.Entities.user?)null);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var act = async () => await service.UpdateAvatarAsync(TestGuids.UserId, CreateFakeFile());

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task UpdateAvatarAsync_ValidFile_UploadsAndUpdatesAvatarUrl()
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var user = new UserBuilder().WithId(TestGuids.UserId).Build();
        userRepo.Setup(r => r.GetByIdWithRolesAsync(TestGuids.UserId)).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var result = await service.UpdateAvatarAsync(TestGuids.UserId, CreateFakeFile());

        result.message.Should().Be("Avatar updated successfully");
        user.avatar_url.Should().Be("https://cdn.example.com/avatar.jpg");
        userRepo.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
