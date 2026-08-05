using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Equivalence Partitioning cho BookingService.GetCabinAvailabilityAsync — xem TestData/Booking/BookingService/GetCabinAvailability.json.
/// Lớp tương đương: lịch không tồn tại, lịch không gắn tàu (trả rỗng), tàu có cabin còn chỗ, tàu có cabin đã full/overbooked (capped).
/// </summary>
public class GetCabinAvailabilityTests
{
    public record GetCabinAvailabilityTestCase(
        string CaseName,
        bool ScheduleExists,
        bool BoatExists,
        bool HasCabin,
        int TotalRooms,
        int BookedRooms,
        int? ExpectedExceptionCode,
        int ExpectedResultCount,
        int? ExpectedAvailableRooms,
        int? ExpectedBookedRooms);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<GetCabinAvailabilityTestCase>("TestData/Booking/BookingService/GetCabinAvailability.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task GetCabinAvailabilityAsync_EquivalencePartitions(GetCabinAvailabilityTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var cabin = c.HasCabin
            ? new BoatCabinBuilder().WithId(TestGuids.CabinId).WithTotalRooms(c.TotalRooms).WithName("Cabin A").Build()
            : null;
        var boat = c.BoatExists
            ? new BoatBuilder().WithCabins(cabin is null ? [] : [cabin]).Build()
            : null;
        var schedule = c.ScheduleExists ? new TourScheduleBuilder().WithBoat(boat).Build() : null;

        bookingRepo.Setup(r => r.FindScheduleWithCabinsAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        bookingRepo.Setup(r => r.GetBookedCabinQuantitiesAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [TestGuids.CabinId] = c.BookedRooms });

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions);

        var act = async () => await service.GetCabinAvailabilityAsync(TestGuids.ScheduleId, CancellationToken.None);

        if (c.ExpectedExceptionCode is not null)
        {
            var exception = await act.Should().ThrowAsync<AppException>();
            exception.Which.ShouldBeAppException(c.ExpectedExceptionCode.Value);
        }
        else
        {
            var result = await act.Should().NotThrowAsync();
            result.Subject.Should().HaveCount(c.ExpectedResultCount);
            if (c.ExpectedResultCount > 0)
            {
                var item = result.Subject.Single();
                item.AvailableRooms.Should().Be(c.ExpectedAvailableRooms!.Value);
                item.BookedRooms.Should().Be(c.ExpectedBookedRooms!.Value);
            }
        }
    }
}
