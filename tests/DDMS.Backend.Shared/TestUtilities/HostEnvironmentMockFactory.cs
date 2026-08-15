using Microsoft.Extensions.Hosting;
using Moq;

namespace DDMS.Backend.Shared.TestUtilities;

public static class HostEnvironmentMockFactory
{
    public static IHostEnvironment Development() => Create(Environments.Development);

    public static IHostEnvironment Production() => Create(Environments.Production);

    private static IHostEnvironment Create(string environmentName)
    {
        var mock = new Mock<IHostEnvironment>();
        mock.SetupGet(e => e.EnvironmentName).Returns(environmentName);
        mock.SetupGet(e => e.ApplicationName).Returns("DDMS.Backend.Tests");
        return mock.Object;
    }
}
