using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Implementations;
using DDMS.Backend.Services.Interfaces;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Builders.RequestBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Equivalence Partitioning cho BookingService.CreateAsync — xem TestData/Booking/BookingService/Create.json.
/// Các lớp tương đương: lịch không tồn tại, tàu bị khoá compliance, đã đặt cùng ngày, không có cabin,
/// cabin không thuộc lịch trình, số lượng cabin không hợp lệ (<=0 hoặc vượt total_rooms), và case thành công
/// (có/không có cabin, có/không có service) để phủ hết nhánh "??"/"Count > 0".
/// </summary>
public class CreateTests
{
    public record CreateBookingTestCase(
        string CaseName,
        bool ScheduleExists,
        bool BoatExists,
        string? ComplianceStatus,
        bool AlreadyBooked,
        bool HasCabins,
        bool CabinsIsEmptyList,
        bool ScheduleWithCabinsExists,
        bool CabinIdMatchesSchedule,
        int RequestedQuantity,
        int BookedQuantity,
        int TotalRooms,
        bool HasServices,
        bool ServicesIsEmptyList,
        int? ExpectedExceptionCode,
        string? ExpectedStatus);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<CreateBookingTestCase>("TestData/Booking/BookingService/Create.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task CreateAsync_EquivalencePartitions(CreateBookingTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var boat = c.BoatExists
            ? new BoatBuilder()
                .WithComplianceStatus(c.ComplianceStatus ?? BoatComplianceStatuses.Valid)
                .WithCabins(c.CabinIdMatchesSchedule
                    ? new[] { new BoatCabinBuilder().WithId(TestGuids.CabinId).WithTotalRooms(c.TotalRooms).Build() }
                    : Array.Empty<boat_cabin>())
                .Build()
            : null;

        var schedule = c.ScheduleExists
            ? new TourScheduleBuilder().WithBoat(boat).DepartingInDays(10).Build()
            : null;

        bookingRepo.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        bookingRepo.Setup(r => r.HasActiveBookingForTourDateAsync(
                TestGuids.UserId, It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(c.AlreadyBooked);

        var scheduleWithCabins = c.ScheduleWithCabinsExists ? schedule : null;
        bookingRepo.Setup(r => r.FindScheduleWithCabinsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduleWithCabins);

        bookingRepo.Setup(r => r.GetBookedCabinQuantitiesAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [TestGuids.CabinId] = c.BookedQuantity });

        var requestBuilder = new CreateBookingRequestBuilder().WithScheduleId(TestGuids.ScheduleId);
        if (c.HasCabins)
        {
            requestBuilder.WithCabin(TestGuids.CabinId, c.RequestedQuantity, 100_000m);
        }

        var request = requestBuilder.Build();
        if (!c.HasCabins && c.CabinsIsEmptyList)
        {
            request.Cabins = new List<Models.DTOs.Booking.CreateBookingCabinRequest>();
        }

        if (c.HasServices)
        {
            request.Services = new List<Models.DTOs.Booking.CreateBookingServiceRequest>
            {
                new() { ServiceId = Guid.NewGuid(), Quantity = 1, UnitPrice = 50_000m }
            };
        }
        else if (c.ServicesIsEmptyList)
        {
            request.Services = new List<Models.DTOs.Booking.CreateBookingServiceRequest>();
        }

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions,
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object,
            BookingPaymentRepositoryMockFactory.Create().Object);

        var act = async () => await service.CreateAsync(TestGuids.UserId, request, CancellationToken.None);

        if (c.ExpectedExceptionCode is not null)
        {
            var exception = await act.Should().ThrowAsync<AppException>();
            exception.Which.ShouldBeAppException(c.ExpectedExceptionCode.Value);
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        else
        {
            var result = await act.Should().NotThrowAsync();
            result.Subject.Status.Should().Be(c.ExpectedStatus);
            result.Subject.ScheduleId.Should().Be(TestGuids.ScheduleId);
            bookingRepo.Verify(r => r.AddBooking(It.IsAny<Models.Entities.booking>()), Times.Once);
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            if (c.HasCabins)
            {
                bookingRepo.Verify(r => r.AddBookingCabin(It.IsAny<booking_cabin>()), Times.Once);
            }

            if (c.HasServices)
            {
                bookingRepo.Verify(r => r.AddBookingService(It.IsAny<booking_service>()), Times.Once);
            }
        }
    }
}
