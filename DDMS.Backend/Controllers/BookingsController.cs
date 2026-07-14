using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;
    private readonly ICurrentUser _user;

    public BookingsController(IBookingService bookings, ICurrentUser user)
    {
        _bookings = bookings;
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

    [HttpGet]
    public async Task<IActionResult> GetUserBookings(CancellationToken ct)
    {
        var result = await _bookings.GetUserBookingsAsync(_user.Id, ct);
        return Ok(ApiResponse<List<UserBookingListItemResponse>>.Ok(result));
    }

    [HttpPut("{id:guid}/pay")]
    public async Task<IActionResult> ConfirmPayment(Guid id, CancellationToken ct)
    {
        await _bookings.ConfirmPaymentAsync(id, _user.Id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(id, _user.Id, ct);
        return Ok(ApiResponse<CancelBookingResult>.Ok(result));
    }
}
