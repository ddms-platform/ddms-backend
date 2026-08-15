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
    private readonly IAdminAlertPublisher _adminAlerts;
    private readonly IBookingPricingService _pricing;
    private readonly IPromotionsRepository _promotions;

    public BookingService(
        IBookingRepository repo,
        IWalletRepository wallets,
        IEmailSender emailSender,
        INotificationService notificationService,
        IOptions<BookingHoldOptions> holdOptions,
        IAdminAlertPublisher adminAlerts,
        IBookingPricingService pricing,
        IPromotionsRepository promotions)
    {
        _repo = repo;
        _wallets = wallets;
        _emailSender = emailSender;
        _notificationService = notificationService;
        _holdOptions = holdOptions.Value;
        _adminAlerts = adminAlerts;
        _pricing = pricing;
        _promotions = promotions;
    }

    /// <summary>Tính giá bằng service dùng chung với endpoint áp mã, nên hai đường luôn ra cùng con số.</summary>
    private Task<BookingQuote> QuoteAsync(CreateBookingRequest request, CancellationToken ct) =>
        _pricing.QuoteAsync(
            request.ScheduleId,
            request.NumPeople,
            (request.Cabins ?? []).Select(c => new BookingLineRequest { Id = c.CabinId, Quantity = c.Quantity }).ToList(),
            (request.Services ?? []).Select(s => new BookingLineRequest { Id = s.ServiceId, Quantity = s.Quantity }).ToList(),
            request.PromotionCode,
            ct);

    /// <summary>
    /// Áp mã giảm giá lên một booking đang chờ thanh toán. Tính lại toàn bộ giá từ DB
    /// thay vì tin số cũ, rồi ghi đè promotion_id/discount_amount/total_price.
    /// Chưa tiêu lượt ở đây — lượt chỉ tính khi khách thanh toán xong.
    /// </summary>
    public async Task<BookingQuote> ApplyPromotionAsync(
        Guid bookingId, Guid userId, string? code, CancellationToken ct)
    {
        var booking = await _repo.FindUserBookingWithLinesAsync(bookingId, userId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy thông tin đặt tour.");

        if (booking.status != BookingStatuses.Pending && booking.status != BookingStatuses.Holding)
            throw new AppException(
                ErrorCode.UncategorizedError, "Đơn này không còn ở trạng thái cho phép đổi mã giảm giá.");

        if (booking.status == BookingStatuses.Holding
            && booking.hold_expired_at != null
            && booking.hold_expired_at <= DateTime.UtcNow)
            throw new AppException(ErrorCode.HoldExpired, ErrorCode.Messages.HoldExpired);

        var quote = await _pricing.QuoteAsync(
            booking.schedule_id,
            booking.num_people,
            booking.booking_cabins.Select(c => new BookingLineRequest { Id = c.cabin_id, Quantity = c.quantity }).ToList(),
            booking.booking_services.Select(s => new BookingLineRequest { Id = s.service_id, Quantity = s.quantity }).ToList(),
            code,
            ct);

        booking.promotion_id = quote.PromotionId;
        booking.base_price = quote.BasePrice;
        booking.cabin_price = quote.CabinPrice;
        booking.service_price = quote.ServicePrice;
        booking.discount_amount = quote.DiscountAmount;
        booking.total_price = quote.TotalPrice;
        booking.updated_at = DateTime.UtcNow;

        await _repo.SaveChangesAsync(ct);
        return quote;
    }

    /// <summary>Đơn giá ghi vào booking_cabin/booking_service lấy từ quote, không lấy từ client.</summary>
    private void AddLines(booking booking, BookingQuote quote, DateTime now)
    {
        foreach (var c in quote.CabinLines)
        {
            _repo.AddBookingCabin(new booking_cabin
            {
                id = Guid.NewGuid(),
                booking_id = booking.id,
                cabin_id = c.Id,
                quantity = c.Quantity,
                unit_price = c.UnitPrice,
                created_at = now
            });
        }

        foreach (var s in quote.ServiceLines)
        {
            _repo.AddBookingService(new booking_service
            {
                id = Guid.NewGuid(),
                booking_id = booking.id,
                service_id = s.Id,
                quantity = s.Quantity,
                unit_price = s.UnitPrice,
                created_at = now
            });
        }
    }

    public async Task<BookingResponse> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct)
    {
        var schedule = await _repo.FindScheduleWithTourAsync(request.ScheduleId, ct)
            ?? throw new AppException(ErrorCode.ScheduleNotFound, "Lịch trình tour không tồn tại.");

        if (BoatComplianceStatuses.IsBlocked(schedule.boat?.compliance_status))
            throw new AppException(ErrorCode.BoatBlockedCompliance, ErrorCode.Messages.BoatBlockedCompliance);

        var scheduleDayStart = schedule.start_time.Date;
        var scheduleDayEnd = scheduleDayStart.AddDays(1);
        var alreadyBooked = await _repo.HasActiveBookingForTourDateAsync(
            userId,
            schedule.tour_id,
            scheduleDayStart,
            scheduleDayEnd,
            ct);

        if (alreadyBooked)
            throw new AppException(
                ErrorCode.UncategorizedError,
                "Bạn đã đặt tour này trong ngày đã chọn. Vui lòng chọn ngày khác hoặc hủy đơn cũ trước khi đặt lại.");

        var requestedCabins = request.Cabins ?? new List<CreateBookingCabinRequest>();
        if (requestedCabins.Count > 0)
        {
            var scheduleWithCabins = await _repo.FindScheduleWithCabinsAsync(request.ScheduleId, ct)
                ?? throw new AppException(ErrorCode.ScheduleNotFound, "Lịch trình tour không tồn tại.");
            var cabinsById = scheduleWithCabins.boat?.boat_cabins.ToDictionary(c => c.id)
                ?? new Dictionary<Guid, boat_cabin>();
            var bookedByCabin = await _repo.GetBookedCabinQuantitiesAsync(request.ScheduleId, ct);

            foreach (var requested in requestedCabins.GroupBy(c => c.CabinId))
            {
                if (!cabinsById.TryGetValue(requested.Key, out var cabin))
                    throw new AppException(ErrorCode.ResourceNotFound, "Cabin không thuộc lịch trình đã chọn.");

                var requestedQuantity = requested.Sum(c => c.Quantity);
                var bookedQuantity = bookedByCabin.GetValueOrDefault(requested.Key);
                if (requestedQuantity <= 0 || bookedQuantity + requestedQuantity > cabin.total_rooms)
                    throw new AppException(ErrorCode.UncategorizedError, "Cabin này đã hết chỗ. Vui lòng chọn cabin khác.");
            }
        }

        var quote = await QuoteAsync(request, ct);

        var now = DateTime.UtcNow;
        var booking = new booking
        {
            id = Guid.NewGuid(),
            user_id = userId,
            schedule_id = request.ScheduleId,
            promotion_id = quote.PromotionId,
            num_people = request.NumPeople,
            base_price = quote.BasePrice,
            cabin_price = quote.CabinPrice,
            service_price = quote.ServicePrice,
            discount_amount = quote.DiscountAmount,
            total_price = quote.TotalPrice,
            status = BookingStatuses.Pending,
            notes = request.Notes,
            created_at = now,
            updated_at = now
        };
        _repo.AddBooking(booking);
        AddLines(booking, quote, now);

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

        var quote = await QuoteAsync(request, ct);

        var booking = new booking
        {
            id = Guid.NewGuid(),
            user_id = userId,
            schedule_id = request.ScheduleId,
            promotion_id = quote.PromotionId,
            num_people = request.NumPeople,
            base_price = quote.BasePrice,
            cabin_price = quote.CabinPrice,
            service_price = quote.ServicePrice,
            discount_amount = quote.DiscountAmount,
            total_price = quote.TotalPrice,
            status = BookingStatuses.Holding,
            hold_expired_at = holdExpiredAt,
            notes = request.Notes,
            created_at = now,
            updated_at = now
        };
        _repo.AddBooking(booking);
        AddLines(booking, quote, now);

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

    public Task<int> CancelExpiredHoldsAsync(CancellationToken ct) =>
        _repo.CancelExpiredHoldsAsync(DateTime.UtcNow, BookingStatuses.CancelReasonHoldExpired, ct);

    public async Task<int> SendHoldRemindersAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var remindBefore = now.AddHours(_holdOptions.ReminderBeforeExpiryHours);
        var holds = await _repo.GetHoldsNeedingReminderAsync(now, remindBefore, RoleNames.Agent, ct);

        var sent = 0;
        foreach (var b in holds)
        {
            try
            {
                await _emailSender.SendHoldReminderEmailAsync(
                    b.user.email,
                    b.user.full_name ?? "Quý khách",
                    b.schedule.tour.name,
                    b.hold_expired_at!.Value);
                b.hold_reminder_sent = true; // đánh dấu để không gửi lại
                sent++;
            }
            catch
            {
                // Lỗi gửi 1 email không làm hỏng cả vòng; sẽ thử lại vòng sau (chưa set cờ).
            }
        }

        if (sent > 0) await _repo.SaveChangesAsync(ct);
        return sent;
    }

    public async Task<List<CabinAvailabilityResponse>> GetCabinAvailabilityAsync(Guid scheduleId, CancellationToken ct)
    {
        var schedule = await _repo.FindScheduleWithCabinsAsync(scheduleId, ct)
            ?? throw new AppException(ErrorCode.ScheduleNotFound, "Lịch trình tour không tồn tại.");

        if (schedule.boat == null)
        {
            return new List<CabinAvailabilityResponse>();
        }

        var bookedByCabin = await _repo.GetBookedCabinQuantitiesAsync(scheduleId, ct);

        return schedule.boat.boat_cabins
            .OrderBy(c => c.name)
            .Select(c =>
            {
                var bookedRooms = bookedByCabin.GetValueOrDefault(c.id);
                var availableRooms = Math.Max(c.total_rooms - bookedRooms, 0);

                return new CabinAvailabilityResponse
                {
                    CabinId = c.id,
                    CabinName = c.name,
                    Capacity = c.capacity,
                    Price = c.price,
                    TotalRooms = c.total_rooms,
                    BookedRooms = Math.Min(bookedRooms, c.total_rooms),
                    AvailableRooms = availableRooms
                };
            })
            .ToList();
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

        // Cho phép xác nhận từ pending hoặc holding (còn hạn). Trạng thái khác thì bỏ qua.
        if (booking.status != BookingStatuses.Pending && booking.status != BookingStatuses.Holding)
            return;

        // Giữ chỗ đã quá hạn -> không cho thanh toán (worker sẽ/đã tự huỷ).
        if (booking.status == BookingStatuses.Holding
            && booking.hold_expired_at != null
            && booking.hold_expired_at <= DateTime.UtcNow)
            throw new AppException(ErrorCode.HoldExpired, ErrorCode.Messages.HoldExpired);

        booking.status = BookingStatuses.Confirmed;
        booking.hold_expired_at = null; // đã xác nhận, không còn thời hạn giữ
        booking.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);

        // Lượt dùng mã chỉ tính khi đơn thực sự được thanh toán, nên giỏ hàng bỏ dở
        // hay giữ chỗ hết hạn không làm hao lượt. Nếu mã vừa hết lượt do người khác
        // thanh toán trước thì bỏ qua: khách đã trả tiền theo giá đã giảm rồi.
        if (booking.promotion_id is not null)
            await _promotions.TryConsumeUsageAsync(booking.promotion_id.Value, ct);

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

        try
        {
            var tourName = booking.schedule.tour.name;
            var bookingCode = booking.id.ToString().Substring(0, 8).ToUpper();
            var startTimeFormatted = booking.schedule.start_time.ToString("HH:mm dd/MM/yyyy");

            // 1. Notify Boat Owner
            if (booking.schedule.boat?.owner_id != null)
            {
                var customerName = booking.user?.full_name ?? "Khách hàng";
                await _notificationService.CreateNotificationAsync(
                    senderId: null,
                    type: "owner",
                    title: "Khách hủy đặt chỗ 🚨",
                    body: $"Khách hàng {customerName} đã hủy {booking.num_people} vé tour {tourName} khởi hành lúc {startTimeFormatted}. Ghế trống đã được mở bán lại.",
                    recipientIds: new List<Guid> { booking.schedule.boat.owner_id.Value },
                    data: JsonSerializer.Serialize(new { bookingId = booking.id }),
                    ct: ct
                );
            }

            // 2. Notify Customer if refund processed
            if (eligibleForRefund)
            {
                await _notificationService.CreateNotificationAsync(
                    senderId: null,
                    type: "system",
                    title: "Hoàn tiền thành công 💸",
                    body: $"Số tiền {booking.total_price:N0} đ cho mã đặt chỗ {bookingCode} đã được hoàn lại thành công vào tài khoản ví của bạn.",
                    recipientIds: new List<Guid> { booking.user_id },
                    data: JsonSerializer.Serialize(new { bookingId = booking.id }),
                    ct: ct
                );
            }
        }
        catch { /* best-effort */ }

        // Fire-and-forget admin alert
        _ = Task.Run(async () =>
        {
            try
            {
                await _adminAlerts.PublishAsync(new Models.DTOs.AdminOps.AlertItem
                {
                    Severity = eligibleForRefund ? "warning" : "info",
                    Title = $"Booking #{bookingId.ToString().Substring(0, 8).ToUpper()} vừa bị huỷ",
                    Detail = $"Giá trị {booking.total_price:N0}đ · {(eligibleForRefund ? "hoàn tiền" : "không hoàn tiền")}",
                });
            }
            catch { /* best-effort */ }
        });

        return new CancelBookingResult
        {
            Status = booking.status,
            Refunded = eligibleForRefund,
            AmountRefunded = eligibleForRefund ? booking.total_price : 0m
        };
    }

    public async Task<CheckInBookingResponse> CheckInAsync(CheckInBookingRequest request, CancellationToken ct)
    {
        var raw = (request.BookingCode ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw))
            throw new AppException(ErrorCode.BookingCheckInInvalidCode, ErrorCode.Messages.BookingCheckInInvalidCode);

        booking? booking = null;
        if (Guid.TryParse(raw, out var bookingId))
            booking = await _repo.FindBookingForCheckInByIdAsync(bookingId, ct);
        else
        {
            var code = raw.Length > 8 ? raw[..8] : raw;
            booking = await _repo.FindBookingForCheckInByCodeAsync(code, ct);
        }

        if (booking == null)
            throw new NotFoundException(ErrorCode.BookingCheckInNotFound, ErrorCode.Messages.BookingCheckInNotFound);

        if (string.Equals(booking.status, BookingStatuses.CheckedIn, StringComparison.OrdinalIgnoreCase))
            throw new AppException(ErrorCode.BookingCheckInAlreadyCheckedIn, ErrorCode.Messages.BookingCheckInAlreadyCheckedIn);

        if (string.Equals(booking.status, BookingStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            if (BookingStatuses.IsOwnerCancelled(booking.cancel_reason))
                throw new AppException(ErrorCode.BookingCheckInOwnerCancelled, ErrorCode.Messages.BookingCheckInOwnerCancelled);
            throw new AppException(ErrorCode.BookingCheckInCancelled, ErrorCode.Messages.BookingCheckInCancelled);
        }

        if (!BookingStatuses.CanCheckIn(booking.status))
        {
            if (string.Equals(booking.status, BookingStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                throw new AppException(ErrorCode.BookingCheckInPending, ErrorCode.Messages.BookingCheckInPending);
            if (string.Equals(booking.status, BookingStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                throw new AppException(ErrorCode.BookingCheckInCompleted, ErrorCode.Messages.BookingCheckInCompleted);
            throw new AppException(ErrorCode.BookingCheckInNotEligible, ErrorCode.Messages.BookingCheckInNotEligible);
        }

        var now = DateTime.UtcNow;
        booking.status = BookingStatuses.CheckedIn;
        booking.updated_at = now;
        await _repo.SaveChangesAsync(ct);

        return new CheckInBookingResponse
        {
            BookingId = booking.id,
            BookingCode = BookingStatuses.ToBookingCode(booking.id),
            CustomerName = booking.user.full_name ?? booking.user.email,
            TourName = booking.schedule.tour.name,
            BoatName = booking.schedule.boat?.name ?? "N/A",
            NumPeople = booking.num_people,
            DepartureTime = booking.schedule.start_time.ToString("HH:mm dd/MM/yyyy"),
            Status = booking.status,
            CheckedInAt = now
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
            BookingCode = BookingStatuses.ToBookingCode(b.id),
            CanShowCheckInQr = BookingStatuses.CanShowCheckInQr(b.status),
            CreatedAt = b.created_at.ToString("o")
        };
    }
}
