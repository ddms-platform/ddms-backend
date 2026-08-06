using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Services.Interfaces;
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
/// Equivalence Partitioning cho AuthService.VerifyEmailAsync — xem TestData/Auth/AuthService/VerifyEmail.json.
/// Lớp tương đương: user không tồn tại sau khi verify token, đã verify từ trước (alreadyVerified=true),
/// và verify lần đầu thành công (alreadyVerified=false).
/// </summary>
public class VerifyEmailTests
{
    public record VerifyEmailTestCase(
        string CaseName,
        bool UserExists,
        bool AlreadyVerified,
        string? ExpectedException,
        string? ExpectedMessage);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<VerifyEmailTestCase>("TestData/Auth/AuthService/VerifyEmail.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task VerifyEmailAsync_EquivalencePartitions(VerifyEmailTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        emailVerificationService.Setup(s => s.VerifyByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new EmailVerificationResult("user@example.com", c.AlreadyVerified));

        var user = c.UserExists ? new UserBuilder().WithEmail("user@example.com").Build() : null;
        userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var act = async () => await service.VerifyEmailAsync(new VerifyEmailRequest { token = "fake-token" });

        if (c.ExpectedException == "InvalidCredentials")
        {
            var ex = await act.Should().ThrowAsync<AppException>();
            ex.Which.ShouldBeAppException(ErrorCode.AuthInvalidCredentials);
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.alreadyVerified.Should().Be(c.AlreadyVerified);
        result.Subject.message.Should().Be(c.ExpectedMessage);
    }
}
