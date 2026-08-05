using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class TokenServiceMockFactory
{
    /// <summary>Mặc định: HashToken là hàm thuần (echo "hash:{input}") để test có thể dự đoán được giá trị hash.</summary>
    public static Mock<ITokenService> Create()
    {
        var mock = new Mock<ITokenService>();
        mock.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns((string raw) => $"hash:{raw}");
        mock.Setup(s => s.GenerateAccessToken(It.IsAny<Models.Entities.user>(), It.IsAny<List<string>>()))
            .Returns("fake-access-token");
        mock.Setup(s => s.GenerateRefreshToken())
            .Returns("fake-refresh-token");
        return mock;
    }
}
