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
/// Equivalence Partitioning cho AuthService.ResendVerificationEmailAsync — xem TestData/Auth/AuthService/ResendVerificationEmail.json.
/// Lớp tương đương: email trống, user không tồn tại (trả message chung, không lộ thông tin),
/// email đã verify trước đó, và gửi lại thành công với IsDevelopment true/false.
/// </summary>
public class ResendVerificationEmailTests
{
    public record ResendVerificationEmailTestCase(
        string CaseName,
        string Email,
        bool UserExists,
        bool AlreadyVerified,
        bool IsDevelopment,
        string? ExpectedException,
        bool ExpectVerificationLink);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<ResendVerificationEmailTestCase>("TestData/Auth/AuthService/ResendVerificationEmail.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ResendVerificationEmailAsync_EquivalencePartitions(ResendVerificationEmailTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create(c.IsDevelopment);
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var user = c.UserExists
            ? new UserBuilder().WithEmail(c.Email.Trim().ToLowerInvariant()).WithEmailVerified(c.AlreadyVerified).Build()
            : null;
        userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var act = async () => await service.ResendVerificationEmailAsync(new ResendVerificationEmailRequest { email = c.Email });

        if (c.ExpectedException == "Validation")
        {
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ShouldBeValidationExceptionWithField("email");
            return;
        }

        if (c.ExpectedException == "AlreadyVerified")
        {
            var ex = await act.Should().ThrowAsync<AppException>();
            ex.Which.ShouldBeAppException(ErrorCode.AuthValidationFailed);
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.message.Should().Be(ErrorCode.Messages.VerificationLinkSent);
        if (c.ExpectVerificationLink)
        {
            result.Subject.verificationLink.Should().NotBeNullOrEmpty();
        }
        else
        {
            result.Subject.verificationLink.Should().BeNull();
        }
    }
}
