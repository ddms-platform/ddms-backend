using System.Text.Json;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Helpers;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    private readonly IWalletRepository _wallets;
    private readonly IEmailSender _emailSender;
    private readonly INotificationService _notificationService;
    private readonly BookingHoldOptions _holdOptions;

    public BookingService(IBookingRepository repo, IWalletRepository wallets, IEmailSender emailSender, INotificationService notificationService, IOptions<BookingHoldOptions> holdOptions)
    {
        _repo = repo;
        _wallets = wallets;
        _emailSender = emailSender;
        _notificationService = notificationService;
        _holdOptions = holdOptions.Value;
    }

    public async Task<BookingResponse> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct)
    {
        var schedule = await _repo.FindScheduleWithTourAsync(request.ScheduleId, ct)
            ?? throw new AppException(ErrorCode.ScheduleNotFound, "Lịch trình tour không tồn tại.");

        if (BoatComplianceStatuses.IsBlocked(schedule.boat?.compliance_status))
            throw new AppException(ErrorCode.BoatBlockedCompliance, ErrorCode.Messages.BoatBlockedCompliance);

        var now = DateTime.UtcNow;
        var booking = new booking
        {
            id = Guid.NewGuid(),
            user_id = userId,
            schedule_id = request.ScheduleId,
            promotion_id = request.PromotionId,
            num_people = request.NumPeople,
            base_price = request.BasePrice,
            cabin_price = request.CabinPrice,
            service_price = request.ServicePrice,
            discount_amount = request.DiscountAmount,
            total_price = request.TotalPrice,
            status = BookingStatuses.Pending,
            notes = request.Notes,
            created_at = now,
            updated_at = now
        };
        _repo.AddBooking(booking);

        foreach (var c in request.Cabins ?? Enumerable.Empty<CreateBookingCabinRequest>())
        {
            _repo.AddBookingCabin(new booking_cabin
            {
                id = Guid.NewGuid(),
                booking_id = booking.id,
                cabin_id = c.CabinId,
                quantity = c.Quantity,
                unit_price = c.UnitPrice,
                created_at = now
            });
        }

        foreach (var s in request.Services ?? Enumerable.Empty<CreateBookingServiceRequest>())
        {
            _repo.AddBookingService(new booking_service
            {
                id = Guid.NewGuid(),
                booking_id = booking.id,
                service_id = s.ServiceId,
                quantity = s.Quantity,
                unit_price = s.UnitPrice,
                created_at = now
            });
        }

        await _repo.SaveChangesAsync(ct);

        return new BookingResponse
        {
            Id = booking.id,
            ScheduleId = booking.schedule_id,
            NumPeople = booking.num_people,
            TotalPrice = booking.total_price,
            Status = booking.status,
            CreatedAt = booking.created_at
        };
    }

    /// <summary>
    /// Giữ chỗ tạm thời (chưa thanh toán). Thời gian giữ tính động theo loại khách
    /// và ngày khởi hành (xem <see cref="HoldPolicy"/>). Quá hạn sẽ bị worker tự huỷ.
    /// </summary>
    public async Task<BookingResponse> HoldAsync(Guid userId, CreateBookingRequest request, CancellationToken ct)
    {
        var schedule = await _repo.FindScheduleWithTourAsync(request.ScheduleId, ct)
            ?? throw new AppException(ErrorCode.ScheduleNotFound, "Lịch trình tour không tồn tại.");

        if (BoatComplianceStatuses.IsBlocked(schedule.boat?.compliance_status))
            throw new AppException(ErrorCode.BoatBlockedCompliance, ErrorCode.Messages.BoatBlockedCompliance);

        var now = DateTime.UtcNow;

        // Đại lý (B2B) được giữ lâu hơn; khách lẻ giữ ngắn. Tính theo ngày khởi hành.
        var isAgent = await _repo.UserHasRoleAsync(userId, RoleNames.Agent, ct);
        var holdDuration = HoldPolicy.CalculateHoldDuration(isAgent, schedule.start_time, now, _holdOptions);

        // Tour khởi hành quá sát → cấm giữ chỗ, phải thanh toán ngay.
        if (holdDuration is null)
            throw new AppException(ErrorCode.HoldNotAllowed, ErrorCode.Messages.HoldNotAllowed);

        var holdExpiredAt = now.Add(holdDuration.Value);

        var booking = new booking
        {
            id = Guid.NewGuid(),
            user_id = userId,
            schedule_id = request.ScheduleId,
            promotion_id = request.PromotionId,
            num_people = request.NumPeople,
            base_price = request.BasePrice,
            cabin_price = request.CabinPrice,
            service_price = request.ServicePrice,
            discount_amount = request.DiscountAmount,
            total_price = request.TotalPrice,
            status = BookingStatuses.Holding,
            hold_expired_at = holdExpiredAt,
            notes = request.Notes,
            created_at = now,
            updated_at = now
        };
        _repo.AddBooking(booking);

        foreach (var c in request.Cabins ?? Enumerable.Empty<CreateBookingCabinRequest>())
        {
            _repo.AddBookingCabin(new booking_cabin
            {
                id = Guid.NewGuid(),
                booking_id = booking.id,
                cabin_id = c.CabinId,
                quantity = c.Quantity,
                unit_price = c.UnitPrice,
                created_at = now
            });
        }

        foreach (var s in request.Services ?? Enumerable.Empty<CreateBookingServiceRequest>())
        {
            _repo.AddBookingService(new booking_service
            {
                id = Guid.NewGuid(),
                booking_id = booking.id,
                service_id = s.ServiceId,
                quantity = s.Quantity,
                unit_price = s.UnitPrice,
                created_at = now
            });
        }

        await _repo.SaveChangesAsync(ct);

        return new BookingResponse
        {
            Id = booking.id,
            ScheduleId = booking.schedule_id,
            NumPeople = booking.num_people,
            TotalPrice = booking.total_price,
            Status = booking.status,
            CreatedAt = booking.created_at,
            HoldExpiredAt = booking.hold_expired_at
        };
    }

    public async Task<List<UserBookingListItemResponse>> GetUserBookingsAsync(Guid userId, CancellationToken ct)
    {
        var bookings = await _repo.GetUserBookingsAsync(userId, ct);
        return bookings.Select(MapListItem).ToList();
    }

    public async Task ConfirmPaymentAsync(Guid bookingId, Guid userId, CancellationToken ct)
    {
        var booking = await _repo.FindUserBookingWithDetailsAsync(bookingId, userId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy thông tin đặt tour.");

        if (booking.status != BookingStatuses.Pending) return;

        booking.status = BookingStatuses.Confirmed;
        booking.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);

        try
        {
            var formattedTime = booking.schedule.start_time.ToString("HH:mm dd/MM/yyyy");
            var bookingCode = booking.id.ToString().Substring(0, 8).ToUpper();
            
            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: "Đặt tour thành công 🎉",
                body: $"Cảm ơn bạn! Tour {booking.schedule.tour.name} khởi hành lúc {formattedTime} đã được xác nhận. Mã đặt chỗ của bạn là {bookingCode}.",
                recipientIds: new List<Guid> { booking.user_id },
                data: JsonSerializer.Serialize(new { bookingId = booking.id }),
                ct: ct
            );

            if (booking.schedule.boat?.owner_id != null)
            {
                await _notificationService.CreateNotificationAsync(
                    senderId: null,
                    type: "owner",
                    title: "Đơn đặt chỗ mới ⚓",
                    body: $"Khách hàng {booking.user.full_name ?? "Khách hàng"} vừa đặt {booking.num_people} vé tour {booking.schedule.tour.name} khởi hành lúc {formattedTime}. Doanh thu tạm tính: {booking.total_price.ToString("N0")} đ.",
                    recipientIds: new List<Guid> { booking.schedule.boat.owner_id.Value },
                    data: JsonSerializer.Serialize(new { bookingId = booking.id }),
                    ct: ct
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }

        try
        {
            await _emailSender.SendBookingStatusEmailAsync(
                booking.user.email,
                booking.user.full_name ?? "Khách hàng",
                booking.id.ToString().Substring(0, 8).ToUpper(),
                booking.schedule.tour.name,
                booking.schedule.boat?.name ?? "N/A",
                booking.schedule.start_time,
                booking.total_price,
                "confirmed",
                null
            );
        }
        catch
        {
            
        }
    }

    public async Task<CancelBookingResult> CancelAsync(Guid bookingId, Guid userId, CancellationToken ct)
    {
        var booking = await _repo.FindUserBookingWithScheduleAsync(bookingId, userId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy thông tin đặt tour.");

        if (string.Equals(booking.status, BookingStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
            throw new AppException(ErrorCode.UncategorizedError, "Đặt tour này đã được hủy trước đó.");

        var isPaid = BookingStatuses.IsPaidLike(booking.status);
        var eligibleForRefund = isPaid
            && booking.schedule.start_time - DateTime.UtcNow >= BookingStatuses.RefundWindow;

        var now = DateTime.UtcNow;
        booking.status = BookingStatuses.Cancelled;
        booking.cancelled_at = now;
        booking.updated_at = now;
        booking.cancel_reason = isPaid
            ? (eligibleForRefund ? BookingStatuses.CancelReasonRefunded : BookingStatuses.CancelReasonNoRefund)
            : BookingStatuses.CancelReasonGeneric;

        if (eligibleForRefund)
            await RefundToWalletAsync(userId, booking.total_price, ct);

        await _repo.SaveChangesAsync(ct);

        return new CancelBookingResult
        {
            Status = booking.status,
            Refunded = eligibleForRefund,
            AmountRefunded = eligibleForRefund ? booking.total_price : 0m
        };
    }

    private async Task RefundToWalletAsync(Guid userId, decimal amount, CancellationToken ct)
    {
        var wallet = await _wallets.FindAsync(userId, ct);
        var now = DateTime.UtcNow;
        if (wallet == null)
        {
            wallet = new user_wallet
            {
                id = Guid.NewGuid(),
                user_id = userId,
                balance = 0m,
                created_at = now,
                updated_at = now
            };
            _wallets.Add(wallet);
        }
        wallet.balance += amount;
        wallet.updated_at = now;
    }

    private static UserBookingListItemResponse MapListItem(booking b)
    {
        var tour = b.schedule.tour;
        var image = tour.tour_images
            .OrderBy(img => img.sort_order)
            .Select(img => img.image_url)
            .FirstOrDefault() ?? BookingStatuses.DefaultTourImage;

        return new UserBookingListItemResponse
        {
            Id = b.id.ToString(),
            TourId = tour.id.ToString(),
            TourTitle_vn = tour.name,
            TourTitle_en = tour.name,
            Location_vn = tour.location ?? BookingStatuses.DefaultLocationVn,
            Location_en = tour.location ?? BookingStatuses.DefaultLocationEn,
            Image = image,
            Date = b.schedule.start_time.ToString("yyyy-MM-dd"),
            Time = $"{b.schedule.start_time:HH:mm} - {b.schedule.end_time:HH:mm}",
            Guests = b.num_people,
            TotalPrice = (double)b.total_price,
            Status = BookingStatuses.ToFrontendStatus(b.status),
            CreatedAt = b.created_at.ToString("o")
        };
    }
}
