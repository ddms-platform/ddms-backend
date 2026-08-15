using DDMS.Backend.Models.DTOs.Chat;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Moq;

namespace DDMS.Backend.Shared.Mocks.Repositories;

public static class ChatRepositoryMockFactory
{
    /// <summary>
    /// Mặc định: booking truyền vào tồn tại, chưa có hội thoại nào cho booking đó.
    /// GetConversationsByUserIdAsync tự trả về những hội thoại đã được
    /// AddConversationAsync tạo trong cùng lượt chạy, để StartConversationAsync
    /// không vỡ ở bước `list.First(...)` cuối hàm.
    /// </summary>
    public static Mock<IChatRepository> Create(booking booking)
    {
        var mock = new Mock<IChatRepository>();
        var created = new List<conversation>();

        mock.Setup(r => r.GetBookingWithTourAndOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        mock.Setup(r => r.GetConversationByBookingIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((conversation?)null);

        mock.Setup(r => r.AddConversationAsync(It.IsAny<conversation>(), It.IsAny<CancellationToken>()))
            .Callback<conversation, CancellationToken>((c, _) => created.Add(c))
            .Returns(Task.CompletedTask);

        mock.Setup(r => r.AddConversationMemberAsync(It.IsAny<conversation_member>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(r => r.GetConversationMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<conversation_member>());

        mock.Setup(r => r.GetConversationsByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => created
                .Select(c => new ConversationResponse
                {
                    Id = c.id,
                    Type = c.type,
                    BookingId = c.booking_id,
                    PartnerName = "",
                    CreatedAt = c.created_at,
                    UpdatedAt = c.updated_at
                })
                .ToList());

        mock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }
}
