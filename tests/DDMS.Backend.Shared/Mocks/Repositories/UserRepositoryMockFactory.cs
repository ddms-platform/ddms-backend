using DDMS.Backend.Repositories.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

public static class UserRepositoryMockFactory
{
    public static Mock<IUserRepository> Create()
    {
        var mock = new Mock<IUserRepository>();
        mock.Setup(r => r.UpdateAsync(It.IsAny<Models.Entities.user>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.MarkEmailVerifiedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.UpdatePasswordHashAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.AssignRoleAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.AddAsync(It.IsAny<Models.Entities.user>()))
            .ReturnsAsync((Models.Entities.user u) => u);
        return mock;
    }
}
