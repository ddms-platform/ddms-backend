using DDMS.Backend.Models.Entities;
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
/// Equivalence Partitioning cho BookingService.GetUserBookingsAsync (bao gồm private MapListItem)
/// — xem TestData/Booking/BookingService/GetUserBookings.json.
/// Phủ nhánh "??" của Image (có/không có tour_image) và Location (có/không có location),
/// đồng thời phủ các trạng thái booking khác nhau (ToFrontendStatus, CanShowCheckInQr).
/// </summary>
public class GetUserBookingsTests
{
    public record GetUserBookingsTestCase(
        string CaseName,
        bool HasBooking,
        bool HasImage,
        bool HasLocation,
        string Status,
        string ExpectedFrontendStatus,
        bool ExpectedCanShowCheckInQr,
        string? ExpectedImage,
        string? ExpectedLocationVn);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<GetUserBookingsTestCase>("TestData/Booking/BookingService/GetUserBookings.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task GetUserBookingsAsync_EquivalencePartitions(GetUserBookingsTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var bookings = new List<booking>();
        if (c.HasBooking)
        {
            var tourBuilder = new TourBuilder();
            if (!c.HasLocation)
            {
                // location null bằng cách build tour rồi set null trực tiếp (builder mặc định có location).
            }
            var tour = tourBuilder.Build();
            tour.location = c.HasLocation ? "Hội An" : null;
            if (c.HasImage)
            {
                tour.tour_images.Add(new TourImageBuilder().WithTourId(tour.id).WithImageUrl("https://example.com/a.jpg").WithSortOrder(1).Build());
            }

            var schedule = new TourScheduleBuilder().WithTour(tour).Build();
            var booking = new BookingBuilder().WithSchedule(schedule).WithStatus(c.Status).Build();
            bookings.Add(booking);
        }

        bookingRepo.Setup(r => r.GetUserBookingsAsync(TestGuids.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions,
            AdminAlertPublisherMockFactory.Create().Object,
            BookingPricingServiceMockFactory.Create().Object,
            PromotionsRepositoryMockFactory.Create().Object,
            BookingPaymentRepositoryMockFactory.Create().Object);

        var result = await service.GetUserBookingsAsync(TestGuids.UserId, CancellationToken.None);

        if (!c.HasBooking)
        {
            result.Should().BeEmpty();
            return;
        }

        var item = result.Should().ContainSingle().Subject;
        item.Status.Should().Be(c.ExpectedFrontendStatus);
        item.CanShowCheckInQr.Should().Be(c.ExpectedCanShowCheckInQr);
        item.Image.Should().Be(c.ExpectedImage);
        item.Location_vn.Should().Be(c.ExpectedLocationVn);
    }
}
