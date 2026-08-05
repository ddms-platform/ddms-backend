using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

/// <summary>Mock IEmailSender no-op — mọi Send*Async đều "gửi thành công" mà không làm gì thật.</summary>
public static class EmailSenderMockFactory
{
    public static Mock<IEmailSender> Create()
    {
        var mock = new Mock<IEmailSender>(MockBehavior.Loose);
        mock.Setup(s => s.SendHoldReminderEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);
        mock.Setup(s => s.SendBookingStatusEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
