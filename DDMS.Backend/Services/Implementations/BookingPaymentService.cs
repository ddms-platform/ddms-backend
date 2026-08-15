using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.DTOs.Billing;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Services.Implementations;

/// <summary>
/// Cổng thanh toán cho booking của khách.
///
/// Nguyên tắc: trạng thái đã-trả-tiền chỉ đến từ PayOS, không bao giờ từ client.
/// Có hai đường dẫn tới đó và cả hai đều đi qua PayOS:
///   1. Webhook PayOS gọi vào — chữ ký được SDK xác minh trước khi ghi.
///   2. Frontend hỏi trạng thái — server tự gọi ngược lên PayOS để đối chiếu.
/// Đường 2 tồn tại vì webhook không tới được máy chạy localhost, và vì webhook
/// có thể trễ hoặc rớt.
/// </summary>
public class BookingPaymentService : IBookingPaymentService
{
    private readonly IBookingRepository _bookings;
    private readonly IBookingPaymentRepository _payments;
    private readonly IBookingService _bookingService;
    private readonly IPayOSGateway _payOS;
    private readonly BillingOptions _billing;
    private readonly ILogger<BookingPaymentService> _logger;

    public BookingPaymentService(
        IBookingRepository bookings,
        IBookingPaymentRepository payments,
        IBookingService bookingService,
        IPayOSGateway payOS,
        IOptions<BillingOptions> billing,
        ILogger<BookingPaymentService> logger)
    {
        _bookings = bookings;
        _payments = payments;
        _bookingService = bookingService;
        _payOS = payOS;
        _billing = billing.Value;
        _logger = logger;
    }

    public async Task<BookingPaymentInitResult> CreatePaymentLinkAsync(
        Guid bookingId, Guid userId, CancellationToken ct)
    {
        EnsurePayOSConfigured();

        var booking = await _bookings.FindUserBookingWithDetailsAsync(bookingId, userId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy thông tin đặt tour.");

        if (booking.status != BookingStatuses.Pending && booking.status != BookingStatuses.Holding)
            throw new AppException(
                ErrorCode.BookingPaymentNotPayable, ErrorCode.Messages.BookingPaymentNotPayable);

        if (booking.status == BookingStatuses.Holding
            && booking.hold_expired_at != null
            && booking.hold_expired_at <= DateTime.UtcNow)
            throw new AppException(ErrorCode.HoldExpired, ErrorCode.Messages.HoldExpired);

        var amount = (int)Math.Round(booking.total_price, MidpointRounding.AwayFromZero);
        if (amount <= 0)
            throw new AppException(
                ErrorCode.BookingPaymentNotPayable, "Đơn này có giá trị bằng 0, không cần thanh toán.");

        // Link cũ còn hiệu lực và đúng số tiền thì dùng lại, tránh mỗi lần F5 lại đẻ một đơn PayOS.
        var existing = await _payments.FindPendingByBookingAsync(bookingId, ct);
        if (existing != null)
        {
            if (existing.amount == booking.total_price
                && !string.IsNullOrWhiteSpace(existing.checkout_url))
            {
                var live = await TryGetPaymentLinkAsync(existing.payos_order_code, ct);
                if (live?.Status == PaymentLinkStatus.Pending)
                {
                    return new BookingPaymentInitResult
                    {
                        CheckoutUrl = existing.checkout_url!,
                        OrderCode = existing.payos_order_code,
                        Amount = existing.amount,
                    };
                }
            }

            // Giá đã đổi (áp mã giảm giá) hoặc link không còn dùng được -> đóng lại.
            existing.status = BookingPaymentStatuses.Cancelled;
            await _payments.SaveChangesAsync(ct);
        }

        var orderCode = NextOrderCode();
        var bookingCode = BookingStatuses.ToBookingCode(bookingId);

        CreatePaymentLinkResponse created;
        try
        {
            created = await _payOS.CreatePaymentLinkAsync(new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                // PayOS giới hạn mô tả 25 ký tự.
                Description = $"DDMS {bookingCode}",
                ReturnUrl = $"{_billing.PayOSBookingReturnUrl}?bookingId={bookingId}",
                CancelUrl = $"{_billing.PayOSBookingCancelUrl}?bookingId={bookingId}",
                BuyerName = booking.user?.full_name,
                BuyerEmail = booking.user?.email,
                BuyerPhone = booking.user?.phone,
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo link PayOS thất bại cho booking {BookingId}", bookingId);
            throw new AppException(
                ErrorCode.BookingPaymentGatewayError, ErrorCode.Messages.BookingPaymentGatewayError);
        }

        _payments.Add(new booking_payment
        {
            id = Guid.NewGuid(),
            booking_id = bookingId,
            amount = booking.total_price,
            amount_paid = 0m,
            status = BookingPaymentStatuses.Pending,
            payos_order_code = orderCode,
            description = $"Thanh toán tour - Đơn {bookingCode}",
            checkout_url = created.CheckoutUrl,
            created_at = DateTime.UtcNow,
        });
        await _payments.SaveChangesAsync(ct);

        return new BookingPaymentInitResult
        {
            CheckoutUrl = created.CheckoutUrl,
            OrderCode = orderCode,
            Amount = booking.total_price,
            QrCode = created.QrCode,
            AccountNumber = created.AccountNumber,
            AccountName = created.AccountName,
            Bin = created.Bin,
            ExpiredAt = created.ExpiredAt.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(created.ExpiredAt.Value).UtcDateTime
                : null,
        };
    }

    public async Task<BookingPaymentStatusResponse> SyncStatusAsync(
        Guid bookingId, Guid userId, CancellationToken ct)
    {
        var booking = await _bookings.FindUserBookingAsync(bookingId, userId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy thông tin đặt tour.");

        var payment = await _payments.FindLatestByBookingAsync(bookingId, ct);
        if (payment == null)
        {
            return new BookingPaymentStatusResponse
            {
                BookingStatus = booking.status,
                PaymentStatus = "none",
                Paid = false,
            };
        }

        if (payment.status == BookingPaymentStatuses.Pending)
        {
            var live = await TryGetPaymentLinkAsync(payment.payos_order_code, ct);
            if (live != null)
                await ApplyGatewayStatusAsync(payment, live.Status, live.AmountPaid, ct);
        }

        var paidAmount = await _payments.GetPaidAmountAsync(bookingId, ct);
        var refreshed = await _bookings.FindUserBookingAsync(bookingId, userId, ct);

        return new BookingPaymentStatusResponse
        {
            BookingStatus = refreshed?.status ?? booking.status,
            PaymentStatus = payment.status,
            Paid = payment.status == BookingPaymentStatuses.Paid,
            OrderCode = payment.payos_order_code,
            AmountPaid = paidAmount,
        };
    }

    public async Task<WebhookHandleResult> HandleWebhookAsync(Webhook body, CancellationToken ct)
    {
        try
        {
            // SDK kiểm tra chữ ký bằng ChecksumKey. Sai chữ ký -> null -> không ghi gì.
            var verified = await _payOS.VerifyWebhookAsync(body, ct);
            if (verified == null)
                return new WebhookHandleResult("01", "Chữ ký không hợp lệ", false);

            var payment = await _payments.FindByOrderCodeAsync(verified.OrderCode, ct);
            if (payment == null)
            {
                // Không phải đơn của luồng booking (có thể là đơn phí chủ tàu). Trả 00 để
                // PayOS thôi gửi lại, nhưng ghi log vì đây cũng có thể là dấu hiệu bất thường.
                _logger.LogWarning(
                    "Webhook PayOS với orderCode {OrderCode} không khớp booking_payment nào", verified.OrderCode);
                return new WebhookHandleResult("00", "Không có đơn tương ứng", true);
            }

            await ApplyGatewayStatusAsync(payment, PaymentLinkStatus.Paid, verified.Amount, ct);
            return new WebhookHandleResult("00", "Thành công", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xử lý webhook PayOS cho booking thất bại");
            return new WebhookHandleResult("99", $"Lỗi xử lý webhook: {ex.Message}", false);
        }
    }

    /// <summary>
    /// Ghi trạng thái PayOS trả về vào bản ghi thanh toán, và nếu đã trả đủ thì xác nhận booking.
    /// Idempotent: gọi lại lần hai (webhook + polling cùng chạy) không xác nhận hai lần.
    /// </summary>
    private async Task ApplyGatewayStatusAsync(
        booking_payment payment, PaymentLinkStatus status, long amountPaid, CancellationToken ct)
    {
        if (BookingPaymentStatuses.IsClosed(payment.status))
            return;

        switch (status)
        {
            case PaymentLinkStatus.Paid:
                payment.status = BookingPaymentStatuses.Paid;
                payment.amount_paid = amountPaid > 0 ? amountPaid : payment.amount;
                payment.paid_at = DateTime.UtcNow;
                await _payments.SaveChangesAsync(ct);
                await _bookingService.MarkPaidAsync(payment.booking_id, ct);
                break;

            case PaymentLinkStatus.Cancelled:
            case PaymentLinkStatus.Failed:
                payment.status = BookingPaymentStatuses.Cancelled;
                await _payments.SaveChangesAsync(ct);
                break;

            case PaymentLinkStatus.Expired:
                payment.status = BookingPaymentStatuses.Expired;
                await _payments.SaveChangesAsync(ct);
                break;

            case PaymentLinkStatus.Underpaid:
                // Trả thiếu: giữ nguyên pending, ghi lại số đã nhận để đối chiếu.
                payment.amount_paid = amountPaid;
                await _payments.SaveChangesAsync(ct);
                _logger.LogWarning(
                    "Booking {BookingId} trả thiếu: nhận {Paid}/{Amount}",
                    payment.booking_id, amountPaid, payment.amount);
                break;
        }
    }

    private Task<PaymentLink?> TryGetPaymentLinkAsync(long orderCode, CancellationToken ct) =>
        _payOS.GetPaymentLinkAsync(orderCode, ct);

    /// <summary>
    /// Mã đơn PayOS phải là số nguyên tăng dần và duy nhất. Dùng mốc thời gian như
    /// luồng phí chủ tàu để hai luồng không đụng mã nhau.
    /// </summary>
    private long NextOrderCode() =>
        (long)(DateTime.UtcNow - _billing.OrderCodeEpoch).TotalMilliseconds;

    private void EnsurePayOSConfigured()
    {
        if (!_payOS.IsConfigured)
            throw new AppException(
                ErrorCode.BookingPaymentNotConfigured, ErrorCode.Messages.BookingPaymentNotConfigured);
    }
}
