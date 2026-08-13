using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

public static class PromotionsRepositoryMockFactory
{
    /// <summary>Mặc định: không có mã nào trong DB, và tiêu lượt luôn thành công.</summary>
    public static Mock<IPromotionsRepository> Create()
    {
        var mock = new Mock<IPromotionsRepository>();
        mock.Setup(r => r.FindByCodeWithCreatorRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((promotion?)null);
        mock.Setup(r => r.TryConsumeUsageAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return mock;
    }
}
