using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Hubs;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Chat.ChatService;

/// <summary>
/// ChatService.StartConversationAsync — owner phải luôn được thêm làm thành viên.
///
/// Hồi quy: trước đây owner chỉ được lấy từ tour.created_by. Tour tạo qua
/// api/legacy/tours (TourService.CreateAsync) không bao giờ gán trường đó, nên
/// ownerId = null và owner bị bỏ qua im lặng — cuộc hội thoại chỉ có một thành
/// viên là khách. Owner không nhận được tin nhắn nào và cũng không thấy cuộc
/// hội thoại trong /inbox, vì danh sách lọc theo conversation_members.
/// </summary>
public class StartConversationTests
{
    private static DDMS.Backend.Services.Implementations.ChatService CreateSut(booking booking, out Mock<DDMS.Backend.Repositories.Interfaces.IChatRepository> repo)
    {
        repo = ChatRepositoryMockFactory.Create(booking);
        return new DDMS.Backend.Services.Implementations.ChatService(
            repo.Object,
            EmailSenderMockFactory.Create().Object,
            new Mock<IHubContext<ChatHub>>().Object,
            new Mock<ICloudinaryService>().Object);
    }

    private static booking BuildBooking(Guid? boatOwnerId, Guid? tourCreatedBy)
    {
        var schedule = new TourScheduleBuilder()
            .WithTour(new TourBuilder().WithCreatedBy(tourCreatedBy).Build())
            .WithBoat(boatOwnerId.HasValue
                ? new BoatBuilder().WithOwnerId(boatOwnerId).Build()
                : null)
            .Build();

        return new BookingBuilder()
            .WithUserId(TestGuids.UserId)
            .WithSchedule(schedule)
            .Build();
    }

    [Fact]
    public async Task StartConversationAsync_LayOwnerTuChuTauCuaLichTrinh()
    {
        // tour.created_by = null (tour legacy) nhưng lịch trình có tàu của owner
        var sut = CreateSut(BuildBooking(boatOwnerId: TestGuids.OwnerId, tourCreatedBy: null), out var repo);

        await sut.StartConversationAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        repo.Verify(r => r.AddConversationMemberAsync(
            It.Is<conversation_member>(m => m.user_id == TestGuids.OwnerId),
            It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.AddConversationMemberAsync(
            It.Is<conversation_member>(m => m.user_id == TestGuids.UserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartConversationAsync_KhongCoTauThiLuiVeTourCreatedBy()
    {
        var sut = CreateSut(BuildBooking(boatOwnerId: null, tourCreatedBy: TestGuids.OwnerId), out var repo);

        await sut.StartConversationAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        repo.Verify(r => r.AddConversationMemberAsync(
            It.Is<conversation_member>(m => m.user_id == TestGuids.OwnerId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartConversationAsync_ChuTauCuaTauDuocUuTienHonTourCreatedBy()
    {
        var sut = CreateSut(BuildBooking(boatOwnerId: TestGuids.OwnerId, tourCreatedBy: TestGuids.OtherUserId), out var repo);

        await sut.StartConversationAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        repo.Verify(r => r.AddConversationMemberAsync(
            It.Is<conversation_member>(m => m.user_id == TestGuids.OwnerId),
            It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.AddConversationMemberAsync(
            It.Is<conversation_member>(m => m.user_id == TestGuids.OtherUserId),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartConversationAsync_KhongXacDinhDuocOwnerThiBaoLoi()
    {
        // Cả hai nguồn đều null — trước đây lặng lẽ tạo hội thoại một thành viên
        var sut = CreateSut(BuildBooking(boatOwnerId: null, tourCreatedBy: null), out var repo);

        var act = async () => await sut.StartConversationAsync(TestGuids.BookingId, TestGuids.UserId, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        repo.Verify(r => r.AddConversationAsync(It.IsAny<conversation>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
