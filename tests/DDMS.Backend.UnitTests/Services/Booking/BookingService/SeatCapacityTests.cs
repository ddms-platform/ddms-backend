using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Builders.RequestBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Sức chứa hiệu dụng của chuyến = min(boat.max_passengers, tour.max_guests), chỉ tính
/// giá trị đã khai (&gt; 0). Trước đây chỉ xét con thuyền, nên chuyến chưa gán thuyền là
/// bỏ qua check overbooking hoàn toàn — đặt bao nhiêu khách cũng qua.
/// </summary>
public class SeatCapacityTests
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

    /// <summary>
    /// Dựng lịch trình với sức chứa thuyền và số khách tối đa của tour khai riêng.
    /// <paramref name="boatMaxPassengers"/> = null nghĩa là chuyến chưa gán thuyền.
    /// </summary>
    private void ArrangeSchedule(int? boatMaxPassengers, int? tourMaxGuests, int bookedSeats)
    {
        var tourEntity = new TourBuilder().WithMaxGuests(tourMaxGuests).Build();
        var scheduleBuilder = new TourScheduleBuilder()
            .WithTour(tourEntity)
            .WithStatus(TourScheduleStatuses.Scheduled)
            .DepartingInDays(10);

        scheduleBuilder = boatMaxPassengers is null
            ? scheduleBuilder.WithNoBoat()
            : scheduleBuilder.WithBoat(new BoatBuilder().WithMaxPassengers(boatMaxPassengers.Value).Build());

        var schedule = scheduleBuilder.Build();

        _bookingRepo.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepo.Setup(r => r.FindScheduleWithCabinsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepo.Setup(r => r.GetBookedSeatsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookedSeats);
    }

    private Task<DDMS.Backend.Models.DTOs.Booking.BookingResponse> Book(int numPeople) =>
        CreateSut().CreateAsync(
            TestGuids.UserId,
            new CreateBookingRequestBuilder().WithNumPeople(numPeople).Build(),
            CancellationToken.None);

    [Fact]
    public async Task CreateAsync_ChanVuotSoKhachToiDaCuaTour_KhiChuyenChuaGanThuyen()
    {
        // Đây là lỗ cũ: không có thuyền → capacity = 0 → return sớm, đặt bao nhiêu cũng qua.
        ArrangeSchedule(boatMaxPassengers: null, tourMaxGuests: 8, bookedSeats: 6);

        var act = async () => await Book(3);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
        _bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ChoDatDenDungSoKhachToiDaCuaTour_KhiChuyenChuaGanThuyen()
    {
        // Biên: 6 + 2 = 8 = max_guests → vẫn phải nhận.
        ArrangeSchedule(boatMaxPassengers: null, tourMaxGuests: 8, bookedSeats: 6);

        var result = await Book(2);

        result.Status.Should().Be(BookingStatuses.Pending);
    }

    [Fact]
    public async Task CreateAsync_CapTheoTour_KhiThuyenToHonSoKhachToiDaKhaiChoTour()
    {
        ArrangeSchedule(boatMaxPassengers: 50, tourMaxGuests: 10, bookedSeats: 0);

        var act = async () => await Book(12);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
    }

    [Fact]
    public async Task CreateAsync_CapTheoThuyen_KhiTourKhaiSoKhachLonHonSucChuaThuyen()
    {
        ArrangeSchedule(boatMaxPassengers: 10, tourMaxGuests: 50, bookedSeats: 0);

        var act = async () => await Book(12);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
    }

    [Fact]
    public async Task CreateAsync_BoQuaKiemTraKhiCaThuyenVaTourDeuChuaKhaiSucChua()
    {
        // Dữ liệu cũ: không thuyền, tour cũng chưa khai. Không lấy dữ liệu thiếu
        // làm cớ chặn hết mọi đơn.
        ArrangeSchedule(boatMaxPassengers: null, tourMaxGuests: null, bookedSeats: 500);

        var result = await Book(2);

        result.Status.Should().Be(BookingStatuses.Pending);
    }

    [Fact]
    public async Task HoldAsync_ChanVuotSoKhachToiDaCuaTour_KhiChuyenChuaGanThuyen()
    {
        ArrangeSchedule(boatMaxPassengers: null, tourMaxGuests: 8, bookedSeats: 6);

        var act = async () => await CreateSut().HoldAsync(
            TestGuids.UserId,
            new CreateBookingRequestBuilder().WithNumPeople(3).Build(),
            CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
        _bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
