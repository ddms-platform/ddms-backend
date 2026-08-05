using DDMS.Backend.Services.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.External;

public static class CloudinaryServiceMockFactory
{
    public static Mock<ICloudinaryService> Create()
    {
        var mock = new Mock<ICloudinaryService>();
        mock.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cdn.example.com/avatar.jpg", "public-id-123"));
        return mock;
    }
}
