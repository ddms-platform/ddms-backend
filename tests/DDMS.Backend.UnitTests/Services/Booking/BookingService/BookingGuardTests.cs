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
/// Các chốt chặn bảo vệ luồng đặt tour, áp cho CẢ hai đường vào (CreateAsync và HoldAsync):
///   · sức chứa tàu — tổng khách đã đặt + khách mới không được vượt max_passengers
///   · phòng còn trống — đường giữ chỗ trước đây bỏ qua hoàn toàn phần kiểm tra này
///   · lịch trình còn nhận đặt — không đặt được tour đã khởi hành hoặc đã bị huỷ
/// </summary>
public class BookingGuardTests
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

    /// <summary>Dựng lịch trình mặc định: tàu 20 khách, khởi hành sau 10 ngày, còn nhận đặt.</summary>
    private tour_schedule ArrangeSchedule(
        int maxPassengers = 20,
        double departingInDays = 10,
        string status = TourScheduleStatuses.Scheduled,
        params boat_cabin[] cabins)
    {
        var boat = new BoatBuilder().WithMaxPassengers(maxPassengers).WithCabins(cabins).Build();
        var schedule = new TourScheduleBuilder()
            .WithBoat(boat)
            .WithStatus(status)
            .DepartingInDays(departingInDays)
            .Build();

        _bookingRepo.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _bookingRepo.Setup(r => r.FindScheduleWithCabinsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        return schedule;
    }

    private void ArrangeBookedSeats(int seats) =>
        _bookingRepo.Setup(r => r.GetBookedSeatsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seats);

    private void ArrangeBookedCabins(int quantity) =>
        _bookingRepo.Setup(r => r.GetBookedCabinQuantitiesAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [TestGuids.CabinId] = quantity });

    private void AssertNothingSaved() =>
        _bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

    // ── ② Sức chứa tàu ────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_TuChoiKhiTongKhachVuotSucChuaTau()
    {
        ArrangeSchedule(maxPassengers: 20);
        ArrangeBookedSeats(18);

        var request = new CreateBookingRequestBuilder().WithNumPeople(3).Build();

        var act = async () => await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
        AssertNothingSaved();
    }

    [Fact]
    public async Task HoldAsync_TuChoiKhiTongKhachVuotSucChuaTau()
    {
        ArrangeSchedule(maxPassengers: 20);
        ArrangeBookedSeats(18);

        var request = new CreateBookingRequestBuilder().WithNumPeople(3).Build();

        var act = async () => await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleSeatsExhausted);
        AssertNothingSaved();
    }

    [Fact]
    public async Task CreateAsync_ChoDatKhiSoKhachVuaDayTau()
    {
        // Biên: 18 + 2 = 20 = sức chứa → vẫn phải nhận.
        ArrangeSchedule(maxPassengers: 20);
        ArrangeBookedSeats(18);

        var request = new CreateBookingRequestBuilder().WithNumPeople(2).Build();

        var result = await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        result.Status.Should().Be(BookingStatuses.Pending);
    }

    [Fact]
    public async Task CreateAsync_BoQuaKiemTraSucChuaKhiTauChuaKhaiBaoSoKhach()
    {
        // Dữ liệu cũ có max_passengers = 0. Không được vin vào đó để chặn hết mọi đơn.
        ArrangeSchedule(maxPassengers: 0);
        ArrangeBookedSeats(500);

        var request = new CreateBookingRequestBuilder().WithNumPeople(2).Build();

        var result = await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        result.Status.Should().Be(BookingStatuses.Pending);
    }

    // ── ④ Phòng còn trống trên đường giữ chỗ ──────────────────

    [Fact]
    public async Task HoldAsync_TuChoiKhiPhongDaHetCho()
    {
        var cabin = new BoatCabinBuilder().WithId(TestGuids.CabinId).WithTotalRooms(2).Build();
        ArrangeSchedule(cabins: cabin);
        ArrangeBookedSeats(0);
        ArrangeBookedCabins(2);

        var request = new CreateBookingRequestBuilder()
            .WithCabin(TestGuids.CabinId, quantity: 1, unitPrice: 100_000m)
            .Build();

        var act = async () => await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
        AssertNothingSaved();
    }

    [Fact]
    public async Task HoldAsync_TuChoiKhiPhongKhongThuocLichTrinh()
    {
        ArrangeSchedule();
        ArrangeBookedSeats(0);

        var request = new CreateBookingRequestBuilder()
            .WithCabin(Guid.NewGuid(), quantity: 1, unitPrice: 100_000m)
            .Build();

        var act = async () => await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ResourceNotFound);
        AssertNothingSaved();
    }

    // ── ⑥ Lịch trình còn nhận đặt ─────────────────────────────

    [Fact]
    public async Task CreateAsync_TuChoiKhiTourDaKhoiHanh()
    {
        ArrangeSchedule(departingInDays: -1);
        ArrangeBookedSeats(0);

        var request = new CreateBookingRequestBuilder().Build();

        var act = async () => await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleDeparted);
        AssertNothingSaved();
    }

    [Fact]
    public async Task HoldAsync_TuChoiKhiTourDaKhoiHanh()
    {
        ArrangeSchedule(departingInDays: -1);
        ArrangeBookedSeats(0);

        var request = new CreateBookingRequestBuilder().Build();

        var act = async () => await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleDeparted);
        AssertNothingSaved();
    }

    [Theory]
    [InlineData("cancelled")]
    [InlineData("CANCELLED")]
    [InlineData("completed")]
    public async Task CreateAsync_TuChoiKhiLichTrinhKhongConNhanDat(string status)
    {
        ArrangeSchedule(status: status);
        ArrangeBookedSeats(0);

        var request = new CreateBookingRequestBuilder().Build();

        var act = async () => await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleNotBookable);
        AssertNothingSaved();
    }

    [Fact]
    public async Task HoldAsync_TuChoiKhiLichTrinhDaHuy()
    {
        ArrangeSchedule(status: "cancelled");
        ArrangeBookedSeats(0);

        var request = new CreateBookingRequestBuilder().Build();

        var act = async () => await CreateSut().HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ScheduleNotBookable);
        AssertNothingSaved();
    }

    [Fact]
    public async Task CreateAsync_VanChoDatVoiTrangThaiLichTrinhLaKhac()
    {
        // Danh sách chặn là deny-list: chỉ chặn trạng thái biết chắc là hỏng.
        // Trạng thái lạ trong dữ liệu cũ không được vô cớ khoá đơn của khách.
        ArrangeSchedule(status: "dang_ban");
        ArrangeBookedSeats(0);

        var request = new CreateBookingRequestBuilder().Build();

        var result = await CreateSut().CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        result.Status.Should().Be(BookingStatuses.Pending);
    }
}
