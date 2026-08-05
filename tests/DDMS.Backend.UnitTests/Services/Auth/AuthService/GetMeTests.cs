using DDMS.Backend.Common.Exceptions;
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
/// Equivalence Partitioning cho AuthService.GetMeAsync — xem TestData/Auth/AuthService/GetMe.json.
/// Lớp tương đương: user không tồn tại, và các trường hợp map dữ liệu (có/không roles, có/không owner
/// profile, email đã/chưa verify) qua nhánh "is not null" và "!= null".
/// </summary>
public class GetMeTests
{
    public record GetMeTestCase(
        string CaseName,
        bool UserExists,
        bool HasRoles,
        bool HasOwnerProfile,
        bool EmailVerified,
        string? ExpectedException);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<GetMeTestCase>("TestData/Auth/AuthService/GetMe.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task GetMeAsync_EquivalencePartitions(GetMeTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var builder = new UserBuilder().WithId(TestGuids.UserId).WithEmailVerified(c.EmailVerified).WithOwnerProfile(c.HasOwnerProfile);
        if (c.HasRoles) builder.WithRoles("user", "agent");
        var user = c.UserExists ? builder.Build() : null;

        userRepo.Setup(r => r.GetByIdWithRolesAsync(TestGuids.UserId)).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var act = async () => await service.GetMeAsync(TestGuids.UserId);

        if (c.ExpectedException == "Unauthorized")
        {
            await act.Should().ThrowAsync<UnauthorizedException>();
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.emailVerified.Should().Be(c.EmailVerified);
        result.Subject.hasOwnerProfile.Should().Be(c.HasOwnerProfile);
        result.Subject.roles.Should().HaveCount(c.HasRoles ? 2 : 0);
    }
}
