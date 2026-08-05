using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.External;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Auth.AuthService;

/// <summary>
/// Equivalence Partitioning cho AuthService.RegisterAsync (bao gồm ValidateRegisterRequest và
/// IsPasswordPolicyValid) — xem TestData/Auth/AuthService/Register.json.
/// Lớp tương đương: từng field bắt buộc thiếu (fullName/email/password/confirmPassword),
/// từng điều kiện của policy mật khẩu bị vi phạm riêng lẻ (độ dài, thường, hoa, số, ký tự đặc biệt),
/// email đã tồn tại, và thành công với IsDevelopment true/false (ảnh hưởng verificationLink trả về).
/// </summary>
public class RegisterTests
{
    public record RegisterTestCase(
        string CaseName,
        string FullName,
        string Email,
        string Password,
        string ConfirmPassword,
        bool EmailAlreadyExists,
        bool IsDevelopment,
        string? ExpectedException,
        string? ExpectedFieldErrorKey,
        bool ExpectVerificationLink);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<RegisterTestCase>("TestData/Auth/AuthService/Register.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task RegisterAsync_EquivalencePartitions(RegisterTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create(c.IsDevelopment);
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(c.EmailAlreadyExists);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var request = new RegisterRequest
        {
            fullName = c.FullName,
            email = c.Email,
            password = c.Password,
            confirmPassword = c.ConfirmPassword
        };

        var act = async () => await service.RegisterAsync(request);

        switch (c.ExpectedException)
        {
            case "Validation":
                var validationEx = await act.Should().ThrowAsync<ValidationException>();
                validationEx.Which.ShouldBeValidationExceptionWithField(c.ExpectedFieldErrorKey!);
                break;
            case "EmailExists":
                var appEx = await act.Should().ThrowAsync<AppException>();
                appEx.Which.ShouldBeAppException(ErrorCode.AuthEmailAlreadyExists);
                break;
            default:
                var result = await act.Should().NotThrowAsync();
                result.Subject.requiresEmailVerification.Should().BeTrue();
                result.Subject.email.Should().Be(c.Email.Trim().ToLowerInvariant());
                if (c.ExpectVerificationLink)
                {
                    result.Subject.verificationLink.Should().NotBeNullOrEmpty();
                }
                else
                {
                    result.Subject.verificationLink.Should().BeNull();
                }
                userRepo.Verify(r => r.AddAsync(It.IsAny<Models.Entities.user>()), Times.Once);
                userRepo.Verify(r => r.AssignRoleAsync(It.IsAny<Guid>(), "user"), Times.Once);
                break;
        }
    }
}
