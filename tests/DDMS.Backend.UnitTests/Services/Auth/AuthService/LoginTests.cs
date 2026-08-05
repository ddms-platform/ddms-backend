using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// Equivalence Partitioning cho AuthService.LoginAsync — xem TestData/Auth/AuthService/Login.json.
/// Lớp tương đương: thiếu email/password (validation), user không tồn tại, user chỉ có tài khoản
/// social (không có password_hash), sai mật khẩu, và đăng nhập thành công.
/// </summary>
public class LoginTests
{
    public record LoginTestCase(
        string CaseName,
        string Email,
        string Password,
        bool UserExists,
        bool HasPasswordHash,
        bool PasswordMatches,
        string? ExpectedException,
        string? ExpectedFieldErrorKey);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<LoginTestCase>("TestData/Auth/AuthService/Login.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task LoginAsync_EquivalencePartitions(LoginTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        const string knownPassword = "Abcdefg1!";
        Models.Entities.user? user = null;
        if (c.UserExists)
        {
            var builder = new UserBuilder().WithEmail(c.Email.Trim().ToLowerInvariant());
            user = c.HasPasswordHash
                ? builder.WithPassword(c.PasswordMatches ? c.Password : knownPassword).Build()
                : builder.WithNoPassword().Build();
        }

        userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var request = new LoginRequest { email = c.Email, password = c.Password };
        var act = async () => await service.LoginAsync(request, "127.0.0.1", "test-agent");

        if (c.ExpectedException == "Validation")
        {
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ShouldBeValidationExceptionWithField(c.ExpectedFieldErrorKey!);
            return;
        }

        if (c.ExpectedException == "InvalidCredentials")
        {
            var ex = await act.Should().ThrowAsync<AppException>();
            ex.Which.ShouldBeAppException(ErrorCode.AuthInvalidCredentials);
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.Should().NotBeNull();
        authSessionService.Verify(s => s.EnsureAccountActive(user!), Times.Once);
        authSessionService.Verify(s => s.EnsureEmailVerified(user!), Times.Once);
        authSessionService.Verify(s => s.IssueTokensAsync(user!, "127.0.0.1", "test-agent"), Times.Once);
    }
}
