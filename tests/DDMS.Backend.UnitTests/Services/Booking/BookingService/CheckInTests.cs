using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Booking;
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
/// Equivalence Partitioning cho BookingService.CheckInAsync — xem TestData/Booking/BookingService/CheckIn.json.
/// Lớp tương đương: mã trống, tra theo Guid/theo code (dài &gt;8 bị cắt / &le;8 giữ nguyên), không tìm thấy,
/// đã check-in, đã huỷ (bởi chủ tàu / bởi khách), pending, completed, trạng thái khác không hợp lệ, và thành công (paid/confirmed).
/// </summary>
public class CheckInTests
{
    public record CheckInTestCase(
        string CaseName,
        string CodeInput,
        bool BookingFound,
        string? Status,
        string? CancelReason,
        bool BoatPresent,
        bool FullNamePresent,
        string? ExpectedExceptionCode);

    public static IEnumerable<object[]> Cases() =>
        JsonDataProvider.LoadAsTheoryData<CheckInTestCase>("TestData/Booking/BookingService/CheckIn.json");

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task CheckInAsync_EquivalencePartitions(CheckInTestCase c)
    {
        var bookingRepo = BookingRepositoryMockFactory.Create();
        var walletRepo = WalletRepositoryMockFactory.Create();
        var emailSender = EmailSenderMockFactory.Create();
        var notificationService = NotificationServiceMockFactory.Create();
        var holdOptions = OptionsFactory.CreateDefault<DDMS.Backend.Configurations.BookingHoldOptions>();

        var actualCode = c.CodeInput == "{NULL}"
            ? null
            : c.CodeInput.Replace("{BOOKING_ID}", TestGuids.BookingId.ToString());

        var boat = c.BoatPresent ? new BoatBuilder().WithName("Rồng Vàng").Build() : null;
        var schedule = new TourScheduleBuilder().WithBoat(boat).Build();
        var user = new UserBuilder().WithFullName("Nguyen Van A").Build();
        if (!c.FullNamePresent) user.full_name = null!;

        var booking = c.BookingFound
            ? new BookingBuilder().WithId(TestGuids.BookingId).WithSchedule(schedule).WithUser(user)
                .WithStatus(c.Status!).WithCancelReason(c.CancelReason).Build()
            : null;

        var normalizedCode = (actualCode ?? string.Empty).Trim();
        if (normalizedCode.Length > 0)
        {
            if (Guid.TryParse(normalizedCode, out _))
            {
                bookingRepo.Setup(r => r.FindBookingForCheckInByIdAsync(TestGuids.BookingId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(booking);
            }
            else
            {
                var expectedLookupCode = normalizedCode.Length > 8 ? normalizedCode[..8] : normalizedCode;
                bookingRepo.Setup(r => r.FindBookingForCheckInByCodeAsync(expectedLookupCode, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(booking);
            }
        }

        var service = new DDMS.Backend.Services.Implementations.BookingService(
            bookingRepo.Object, walletRepo.Object, emailSender.Object, notificationService.Object, holdOptions);

        var request = new CheckInBookingRequest { BookingCode = actualCode! };
        var act = async () => await service.CheckInAsync(request, CancellationToken.None);

        if (c.ExpectedExceptionCode is not null)
        {
            var exception = await act.Should().ThrowAsync<AppException>();
            exception.Which.Code.Should().Be(int.Parse(c.ExpectedExceptionCode));
            return;
        }

        var result = await act.Should().NotThrowAsync();
        result.Subject.Status.Should().Be("checked_in");
        result.Subject.BoatName.Should().Be(c.BoatPresent ? "Rồng Vàng" : "N/A");
        result.Subject.CustomerName.Should().Be(c.FullNamePresent ? "Nguyen Van A" : user.email);
        bookingRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
