using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class AuthSessionServiceMockFactory
{
    /// <summary>
    /// Mặc định: user hợp lệ (EnsureAccountActive/EnsureEmailVerified không throw),
    /// IssueTokensAsync trả về 1 AuthTokensResponse hợp lệ.
    /// </summary>
    public static Mock<IAuthSessionService> Create()
    {
        var mock = new Mock<IAuthSessionService>();
        mock.Setup(s => s.IssueTokensAsync(It.IsAny<Models.Entities.user>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(new AuthTokensResponse
            {
                token = "fake-access-token",
                accessToken = "fake-access-token",
                refreshToken = "fake-refresh-token",
                authenticated = true,
                expiresInSeconds = 3600
            });
        return mock;
    }
}
