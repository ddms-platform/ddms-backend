using Microsoft.Extensions.Hosting;
using Moq;

namespace DDMS.Backend.Shared.Mocks.External;

/// <summary>
/// AuthService dùng IHostEnvironment.IsDevelopment() để quyết định có trả verificationLink
/// (link xác thực email / reset password) trực tiếp trong response hay không (chỉ bật ở dev).
/// </summary>
public static class HostEnvironmentMockFactory
{
    public static Mock<IHostEnvironment> Create(bool isDevelopment = true)
    {
        var mock = new Mock<IHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(isDevelopment ? Environments.Development : Environments.Production);
        return mock;
    }
}
