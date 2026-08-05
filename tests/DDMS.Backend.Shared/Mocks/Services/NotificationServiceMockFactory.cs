using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class NotificationServiceMockFactory
{
    public static Mock<INotificationService> Create()
    {
        var mock = new Mock<INotificationService>();
        mock.Setup(s => s.CreateNotificationAsync(
                It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<Guid>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
