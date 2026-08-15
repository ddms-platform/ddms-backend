using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;
    private readonly IBookingPaymentService _payments;
    private readonly ICurrentUser _user;

    public BookingsController(
        IBookingService bookings,
        IBookingPaymentService payments,
        ICurrentUser user)
    {
        _bookings = bookings;
        _payments = payments;
        _user = user;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookings.CreateAsync(_user.Id, request, ct);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    /// <summary>Giữ chỗ tạm thời (chưa thanh toán) — trả về hold_expired_at để FE đếm ngược.</summary>
    [HttpPost("hold")]
    public async Task<IActionResult> Hold([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookings.HoldAsync(_user.Id, request, ct);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet("schedules/{scheduleId:guid}/cabins")]
    public async Task<IActionResult> GetCabinAvailability(Guid scheduleId, CancellationToken ct)
    {
        var result = await _bookings.GetCabinAvailabilityAsync(scheduleId, ct);
        return Ok(ApiResponse<List<CabinAvailabilityResponse>>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetUserBookings(CancellationToken ct)
    {
        var result = await _bookings.GetUserBookingsAsync(_user.Id, ct);
        return Ok(ApiResponse<List<UserBookingListItemResponse>>.Ok(result));
    }

    /// <summary>Áp mã giảm giá lên đơn đang chờ thanh toán. Trả về bảng giá đã tính lại.</summary>
    [HttpPut("{id:guid}/promotion")]
    public async Task<IActionResult> ApplyPromotion(
        Guid id, [FromBody] ApplyPromotionRequest request, CancellationToken ct)
    {
        var result = await _bookings.ApplyPromotionAsync(id, _user.Id, request.Code, ct);
        return Ok(ApiResponse<BookingQuote>.Ok(result));
    }

    /// <summary>Gỡ mã giảm giá khỏi đơn và trả giá về như cũ.</summary>
    [HttpDelete("{id:guid}/promotion")]
    public async Task<IActionResult> RemovePromotion(Guid id, CancellationToken ct)
    {
        var result = await _bookings.ApplyPromotionAsync(id, _user.Id, null, ct);
        return Ok(ApiResponse<BookingQuote>.Ok(result));
    }

    /// <summary>Tạo link thanh toán PayOS cho đơn. Khách trả tiền trên cổng, không tự khai báo.</summary>
    [HttpPost("{id:guid}/payment-link")]
    public async Task<IActionResult> CreatePaymentLink(Guid id, CancellationToken ct)
    {
        var result = await _payments.CreatePaymentLinkAsync(id, _user.Id, ct);
        return Ok(ApiResponse<BookingPaymentInitResult>.Ok(result));
    }

    /// <summary>
    /// Đối chiếu với PayOS rồi trả về trạng thái thật của đơn.
    /// Frontend hỏi endpoint này thay cho việc tự báo "tôi đã trả".
    /// </summary>
    [HttpGet("{id:guid}/payment-status")]
    public async Task<IActionResult> GetPaymentStatus(Guid id, CancellationToken ct)
    {
        var result = await _payments.SyncStatusAsync(id, _user.Id, ct);
        return Ok(ApiResponse<BookingPaymentStatusResponse>.Ok(result));
    }

    /// <summary>PayOS gọi vào đây. Chữ ký được xác minh trước khi ghi bất cứ thứ gì.</summary>
    [AllowAnonymous]
    [HttpPost("payments/webhook")]
    public async Task<IActionResult> HandlePaymentWebhook([FromBody] Webhook body, CancellationToken ct)
    {
        var r = await _payments.HandleWebhookAsync(body, ct);
        var payload = new { code = r.Code, desc = r.Desc };
        return r.IsSuccess ? Ok(payload) : BadRequest(payload);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(id, _user.Id, ct);
        return Ok(ApiResponse<CancelBookingResult>.Ok(result));
    }
}
