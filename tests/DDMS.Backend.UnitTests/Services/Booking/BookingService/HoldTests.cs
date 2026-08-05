using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Builders.RequestBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.DataProviders;
using DDMS.Backend.Shared.Mocks.Repositories;
using DDMS.Backend.Shared.Mocks.Services;
using DDMS.Backend.Shared.TestUtilities;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingService;

/// <summary>
/// Equivalence Partitioning cho BookingService.HoldAsync — xem TestData/Booking/BookingService/Hold.json.
/// Lớp tương đương chính: lịch không tồn tại, tàu bị khoá compliance, tour khởi hành quá sát (cấm giữ chỗ),
/// và các mức giữ chỗ hợp lệ theo HoldPolicy (B2C, B2B xa/trung/gần) — đồng thời phủ nhánh "??" của Cabins/Services.
/// </summary>
public class HoldTests
{
    public record HoldTestCase(
        string CaseName,
        bool ScheduleExists,
        bool BoatExists,
        string? ComplianceStatus,
        double DepartingInDays,
        bool IsAgent,
        bool HasCabins,
        bool HasServices,
        int? ExpectedExceptionCode,
        string? ExpectedStatus,
        bool ExpectHoldExpiredAt);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<HoldTestCase>("TestData/Booking/BookingService/Hold.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task HoldAsync_EquivalencePartitions(HoldTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var boat = c.BoatExists
            ? new BoatBuilder().WithComplianceStatus(c.ComplianceStatus ?? BoatComplianceStatuses.Valid).Build()
            : null;
        var schedule = c.ScheduleExists
            ? new TourScheduleBuilder().WithBoat(boat).DepartingInDays(c.DepartingInDays).Build()
            : null;

        bookingRepo.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        bookingRepo.Setup(r => r.UserHasRoleAsync(TestGuids.UserId, RoleNames.Agent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(c.IsAgent);

        var requestBuilder = new CreateBookingRequestBuilder().WithScheduleId(TestGuids.ScheduleId);
        if (c.HasCabins)
        {
            requestBuilder.WithCabin(TestGuids.CabinId, 1, 100_000m);
        }
        var request = requestBuilder.Build();
        if (c.HasServices)
        {
            request.Services = new List<Models.DTOs.Booking.CreateBookingServiceRequest>
            {
                new() { ServiceId = Guid.NewGuid(), Quantity = 1, UnitPrice = 50_000m }
            };
        }

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions);

        var act = async () => await service.HoldAsync(TestGuids.UserId, request, CancellationToken.None);

        if (c.ExpectedExceptionCode is not null)
        {
            var exception = await act.Should().ThrowAsync<AppException>();
            exception.Which.ShouldBeAppException(c.ExpectedExceptionCode.Value);
        }
        else
        {
            var result = await act.Should().NotThrowAsync();
            result.Subject.Status.Should().Be(c.ExpectedStatus);
            if (c.ExpectHoldExpiredAt)
            {
                result.Subject.HoldExpiredAt.Should().NotBeNull();
            }
            bookingRepo.Verify(r => r.AddBooking(It.IsAny<booking>()), Times.Once);
            bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
