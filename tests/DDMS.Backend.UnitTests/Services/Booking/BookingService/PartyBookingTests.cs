using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Ghi thành phần đoàn khách vào đơn, và quan hệ giữa ba hạng vé với num_people.
/// num_people v/ẫn là tổng — sức chứa, thông báo cho owner và check-in đều đọc nó.
/// </summary>
public class PartyBookingTests
{
    private readonly Mock<DDMS.Backend.Repositories.Interfaces.IBookingRepository> _bookingRepo =
        BookingRepositoryMockFactory.Create();

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

    private void ArrangeSchedule(int maxPassengers = 20, int bookedSeats = 0)
    {
        var boat = new BoatBuilder().WithMaxPassengers(maxPassengers).Build();
        var schedule = new TourScheduleBuilder().WithBoat(boat).DepartingInDays(10).Build();

        _bookingRepo.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepo.Setup(r => r.FindScheduleWithCabinsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepo.Setup(r => r.GetBookedSeatsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookedSeats);
    }

    private booking CaptureSavedBooking()
    {
        booking? saved = null;
        _bookingRepo.Setup(r => r.AddBooking(It.IsAny<booking>())).Callback<booking>(b => saved = b);
        return saved!;
    }

    [Fact]
    public async Task CreateAsync_LuuDuBaHangVeVaTongKhach()
    {
        ArrangeSchedule();
        booking? saved = null;
        _bookingRepo.Setup(r => r.AddBooking(It.IsAny<booking>())).Callback<booking>(b => saved = b);

        var request = new CreateBookingRequest
        {
            ScheduleId = TestGuids.ScheduleId,
            NumAdults = 2,
            NumChildren = 1,
            NumInfants = 1,
        };

        await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        saved.Should().NotBeNull();
        saved!.num_adults.Should().Be(2);
        saved.num_children.Should().Be(1);
        saved.num_infants.Should().Be(1);
        saved.num_people.Should().Be(4);
    }

    [Fact]
    public async Task CreateAsync_ClientCu_ThiTatCaVaoHangNguoiLon()
    {
        ArrangeSchedule();
        booking? saved = null;
        _bookingRepo.Setup(r => r.AddBooking(It.IsAny<booking>())).Callback<booking>(b => saved = b);

        var request = new CreateBookingRequest { ScheduleId = TestGuids.ScheduleId, NumPeople = 3 };

        await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        saved!.num_adults.Should().Be(3);
        saved.num_children.Should().Be(0);
        saved.num_infants.Should().Be(0);
        saved.num_people.Should().Be(3);
    }

    [Fact]
    public async Task HoldAsync_CungLuuDuBaHangVe()
    {
        ArrangeSchedule();
        booking? saved = null;
        _bookingRepo.Setup(r => r.AddBooking(It.IsAny<booking>())).Callback<booking>(b => saved = b);

        var request = new CreateBookingRequest
        {
            ScheduleId = TestGuids.ScheduleId,
            NumAdults = 1,
            NumChildren = 2,
        };

        await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        saved!.num_adults.Should().Be(1);
        saved.num_children.Should().Be(2);
        saved.num_people.Should().Be(3);
    }

    [Fact]
    public async Task SucChua_DemCaEmBe()
    {
        // Tàu 20 chỗ, đã kín 18. Đoàn 1 người lớn + 2 em bé là 3 người, không nhét vừa.
        ArrangeSchedule(maxPassengers: 20, bookedSeats: 18);

        var request = new CreateBookingRequest
        {
            ScheduleId = TestGuids.ScheduleId,
            NumAdults = 1,
            NumInfants = 2,
        };

        var act = async () => await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
    }

    [Fact]
    public async Task DoanRong_BiTuChoi()
    {
        ArrangeSchedule();

        var request = new CreateBookingRequest { ScheduleId = TestGuids.ScheduleId };

        var act = async () => await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        _bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
