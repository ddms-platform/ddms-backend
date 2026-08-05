using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// Equivalence Partitioning cho AuthService.ForgotPasswordAsync — xem TestData/Auth/AuthService/ForgotPassword.json.
/// Lớp tương đương: email trống (validation) và thành công với IsDevelopment true/false.
/// </summary>
public class ForgotPasswordTests
{
    public record ForgotPasswordTestCase(
        string CaseName,
        string Email,
        bool IsDevelopment,
        string? ExpectedException,
        bool ExpectVerificationLink);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<ForgotPasswordTestCase>("TestData/Auth/AuthService/ForgotPassword.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ForgotPasswordAsync_EquivalencePartitions(ForgotPasswordTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create(c.IsDevelopment);
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var act = async () => await service.ForgotPasswordAsync(new ForgotPasswordRequest { email = c.Email });

        if (c.ExpectedException == "Validation")
        {
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ShouldBeValidationExceptionWithField("email");
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.message.Should().Be(ErrorCode.Messages.PasswordResetLinkSent);
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
