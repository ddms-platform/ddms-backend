using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
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
/// Equivalence Partitioning cho AuthService.ChangePasswordAsync — xem TestData/Auth/AuthService/ChangePassword.json.
/// Lớp tương đương: các lỗi validation (currentPassword/newPassword rỗng hoặc sai policy/confirmPassword lệch),
/// user không tồn tại, tài khoản social không có mật khẩu, current password sai, mật khẩu mới trùng mật khẩu cũ,
/// và đổi mật khẩu thành công.
/// </summary>
public class ChangePasswordTests
{
    public record ChangePasswordTestCase(
        string CaseName,
        string CurrentPassword,
        string NewPassword,
        string ConfirmPassword,
        bool UserExists,
        bool HasPasswordHash,
        string? ActualCurrentPassword,
        string? ExpectedException,
        string? ExpectedFieldErrorKey);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<ChangePasswordTestCase>("TestData/Auth/AuthService/ChangePassword.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ChangePasswordAsync_EquivalencePartitions(ChangePasswordTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var builder = new UserBuilder().WithId(TestGuids.UserId);
        var user = c.UserExists
            ? (c.HasPasswordHash ? builder.WithPassword(c.ActualCurrentPassword!).Build() : builder.WithNoPassword().Build())
            : null;

        userRepo.Setup(r => r.GetByIdWithRolesAsync(TestGuids.UserId)).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var request = new ChangePasswordRequest
        {
            currentPassword = c.CurrentPassword,
            newPassword = c.NewPassword,
            confirmPassword = c.ConfirmPassword
        };
        var act = async () => await service.ChangePasswordAsync(TestGuids.UserId, request);

        switch (c.ExpectedException)
        {
            case "Validation":
                var validationEx = await act.Should().ThrowAsync<ValidationException>();
                validationEx.Which.ShouldBeValidationExceptionWithField(c.ExpectedFieldErrorKey!);
                break;
            case "Unauthorized":
                await act.Should().ThrowAsync<UnauthorizedException>();
                break;
            case "SocialAccount":
                var appEx = await act.Should().ThrowAsync<AppException>();
                appEx.Which.ShouldBeAppException(ErrorCode.AuthValidationFailed);
                break;
            default:
                var result = await act.Should().NotThrowAsync();
                result.Subject.message.Should().Be(ErrorCode.Messages.ChangePasswordSuccess);
                userRepo.Verify(r => r.UpdatePasswordHashAsync(TestGuids.UserId, It.IsAny<string>()), Times.Once);
                refreshTokenRepo.Verify(r => r.RevokeAllActiveForUserAsync(TestGuids.UserId), Times.Once);
                break;
        }
    }
}
