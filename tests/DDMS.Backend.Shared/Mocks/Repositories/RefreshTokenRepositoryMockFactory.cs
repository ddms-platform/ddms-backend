using DDMS.Backend.Repositories.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

public static class RefreshTokenRepositoryMockFactory
{
    public static Mock<IRefreshTokenRepository> Create()
    {
        var mock = new Mock<IRefreshTokenRepository>();
        mock.Setup(r => r.RevokeAsync(It.IsAny<Models.Entities.refresh_token>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.RevokeAllActiveForUserAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.EnforceSessionLimitAsync(It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.AddAsync(It.IsAny<Models.Entities.refresh_token>()))
            .ReturnsAsync((Models.Entities.refresh_token t) => t);
        return mock;
    }
}
