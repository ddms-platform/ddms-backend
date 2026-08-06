using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Auth;
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
/// Equivalence Partitioning cho AuthService.UpdateProfileAsync — xem TestData/Auth/AuthService/UpdateProfile.json.
/// Lớp tương đương: user không tồn tại, phone/address trống (chuẩn hoá thành null), và phone/address có giá trị (được trim).
/// </summary>
public class UpdateProfileTests
{
    public record UpdateProfileTestCase(
        string CaseName,
        bool UserExists,
        string FullName,
        string? Phone,
        string? Address,
        string? ExpectedException,
        string? ExpectedPhone,
        string? ExpectedAddress);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<UpdateProfileTestCase>("TestData/Auth/AuthService/UpdateProfile.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task UpdateProfileAsync_EquivalencePartitions(UpdateProfileTestCase c)
    {
        var userRepo = UserRepositoryMockFactory.Create();
        var refreshTokenRepo = RefreshTokenRepositoryMockFactory.Create();
        var tokenService = TokenServiceMockFactory.Create();
        var authSessionService = AuthSessionServiceMockFactory.Create();
        var emailVerificationService = EmailVerificationServiceMockFactory.Create();
        var passwordResetService = PasswordResetServiceMockFactory.Create();
        var hostEnvironment = HostEnvironmentMockFactory.Create();
        var cloudinaryService = CloudinaryServiceMockFactory.Create();

        var user = c.UserExists ? new UserBuilder().WithId(TestGuids.UserId).Build() : null;
        userRepo.Setup(r => r.GetByIdWithRolesAsync(TestGuids.UserId)).ReturnsAsync(user);

        var service = new DDMS.Backend.Services.Implementations.AuthService(
            userRepo.Object, refreshTokenRepo.Object, tokenService.Object, authSessionService.Object,
            emailVerificationService.Object, passwordResetService.Object, hostEnvironment.Object, cloudinaryService.Object);

        var request = new UpdateProfileRequest { fullName = c.FullName, phone = c.Phone, address = c.Address };
        var act = async () => await service.UpdateProfileAsync(TestGuids.UserId, request);

        if (c.ExpectedException == "Unauthorized")
        {
            await act.Should().ThrowAsync<UnauthorizedException>();
            return;
        }

        await act.Should().NotThrowAsync();
        user!.phone.Should().Be(c.ExpectedPhone);
        user.address.Should().Be(c.ExpectedAddress);
        user.full_name.Should().Be(c.FullName.Trim());
        userRepo.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
