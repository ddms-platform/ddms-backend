using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

public static class WalletRepositoryMockFactory
{
    /// <summary>Mặc định: user chưa có ví (FindAsync trả null) — RefundToWalletAsync sẽ tự tạo ví mới.</summary>
    public static Mock<IWalletRepository> Create()
    {
        var mock = new Mock<IWalletRepository>();
        mock.Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((user_wallet?)null);
        mock.Setup(r => r.Add(It.IsAny<user_wallet>()));
        return mock;
    }
}
