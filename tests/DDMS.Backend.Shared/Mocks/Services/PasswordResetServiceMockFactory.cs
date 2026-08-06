using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class PasswordResetServiceMockFactory
{
    public static Mock<IPasswordResetService> Create()
    {
        var mock = new Mock<IPasswordResetService>();
        mock.Setup(s => s.SendResetLinkAsync(It.IsAny<string>()))
            .ReturnsAsync("https://ddms.test/reset?token=fake-token");
        mock.Setup(s => s.ResetPasswordByTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
