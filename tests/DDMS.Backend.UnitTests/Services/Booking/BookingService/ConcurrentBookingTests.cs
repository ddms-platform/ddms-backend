using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Hai khách bấm đặt chỗ cuối cùng cùng lúc.
///
/// Kiểm tra rồi mới ghi, không có gì chặn ở giữa, thì cả hai đều qua cửa và tàu
/// bán quá chỗ. Chốt chặn là khoá dòng lịch trình trong DB trước khi đếm, nên
/// hai request cùng một lịch trình bị xếp hàng; lịch trình khác nhau vẫn chạy song song.
///
/// Test ở đây khoá lại CƠ CHẾ (có mở giao dịch, có khoá, đúng thứ tự, có commit) —
/// việc DB thật sự tuần tự hoá thì chỉ chạy thật mới chứng minh được.
/// </summary>
public class ConcurrentBookingTests
{
    private readonly Mock<DDMS.Backend.Repositories.Interfaces.IBookingRepository> _bookingRepo =
        BookingRepositoryMockFactory.Create();
    private readonly List<string> _calls = [];

    private DDMS.Backend.Services.Implementations.BookingService CreateSut() =>
        new(
            _bookingRepo.Object,
            WalletRepositoryMockFactory.Create().Object,
            EmailSenderMockFactory.Create().Object,
            NotificationServiceMockFactory.Create().Object,
            OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>(),
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object,
            BookingPaymentRepositoryMockFactory.Create().Object);

    private Mock<IDbContextTransaction> ArrangeSchedule(int maxPassengers = 20, int bookedSeats = 0)
    {
        var boat = new BoatBuilder().WithMaxPassengers(maxPassengers).Build();
        var schedule = new TourScheduleBuilder().WithBoat(boat).DepartingInDays(10).Build();

        _bookingRepo.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepo.Setup(r => r.FindScheduleWithCabinsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var tx = new Mock<IDbContextTransaction>();
        tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("commit")).Returns(Task.CompletedTask);
        tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("rollback")).Returns(Task.CompletedTask);
        tx.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        _bookingRepo.Setup(r => r.CreateExecutionStrategy())
            .Callback(() => _calls.Add("strategy")).Returns(new ImmediateExecutionStrategy());
        _bookingRepo.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("begin")).ReturnsAsync(tx.Object);
        _bookingRepo.Setup(r => r.LockScheduleAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("lock")).Returns(Task.CompletedTask);
        _bookingRepo.Setup(r => r.GetBookedSeatsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("count")).ReturnsAsync(bookedSeats);
        _bookingRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save")).Returns(Task.CompletedTask);

        return tx;
    }

    [Fact]
    public async Task CreateAsync_KhoaLichTrinhTruocKhiDemChoConTrong()
    {
        ArrangeSchedule();
        var request = new CreateBookingRequest { ScheduleId = TestGuids.ScheduleId, NumAdults = 2 };

        await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        // Đếm sau khi khoá thì con số đếm được mới còn đúng lúc ghi.
        // "strategy" đứng trước "begin": EnableRetryOnFailure đang bật nên transaction
        // tự mở phải nằm trong execution strategy, không thì EF ném lỗi ngay.
        _calls.Should().ContainInOrder("strategy", "begin", "lock", "count", "save", "commit");
    }

    [Fact]
    public async Task HoldAsync_CungKhoaLichTrinh()
    {
        ArrangeSchedule();
        var request = new CreateBookingRequest { ScheduleId = TestGuids.ScheduleId, NumAdults = 2 };

        await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        _calls.Should().ContainInOrder("strategy", "begin", "lock", "count", "save", "commit");
    }

    [Fact]
    public async Task HetChoThiKhongCommit()
    {
        ArrangeSchedule(maxPassengers: 20, bookedSeats: 19);
        var request = new CreateBookingRequest { ScheduleId = TestGuids.ScheduleId, NumAdults = 2 };

        var act = async () => await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        _calls.Should().NotContain("commit");
        _calls.Should().NotContain("save");
    }
}
