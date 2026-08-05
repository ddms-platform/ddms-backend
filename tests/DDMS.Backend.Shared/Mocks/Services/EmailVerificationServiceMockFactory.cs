using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class EmailVerificationServiceMockFactory
{
    public static Mock<IEmailVerificationService> Create()
    {
        var mock = new Mock<IEmailVerificationService>();
        mock.Setup(s => s.SendVerificationLinkAsync(It.IsAny<string>()))
            .ReturnsAsync("https://ddms.test/verify?token=fake-token");
        return mock;
    }
}
