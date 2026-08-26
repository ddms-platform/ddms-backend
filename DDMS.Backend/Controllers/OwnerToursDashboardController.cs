using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Models.DTOs.OwnerToursDashboard;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/owner/tours-dashboard")]
public class OwnerToursDashboardController : ControllerBase
{
    private readonly IOwnerToursDashboardService _svc;
    private readonly ICurrentUser _user;

    public OwnerToursDashboardController(IOwnerToursDashboardService svc, ICurrentUser user)
    {
        _svc = svc;
        _user = user;
    }

    [HttpGet("stats")]
    public Task<IActionResult> GetStats(CancellationToken ct) =>
        Wrap(async () => new { isSuccess = true, result = await _svc.GetStatsAsync(_user.Id, ct) });

    [HttpGet("schedules")]
    public Task<IActionResult> GetSchedules(int month, int year, CancellationToken ct) =>
        Wrap(async () => new { isSuccess = true, result = await _svc.GetSchedulesAsync(_user.Id, month, year, ct) });

    [HttpGet("tours")]
    public Task<IActionResult> GetTours(CancellationToken ct) =>
        Wrap(async () => new { isSuccess = true, result = await _svc.GetOwnerToursAsync(_user.Id, ct) });

    [HttpGet("recent-bookings")]
    public Task<IActionResult> GetRecentBookings(CancellationToken ct) =>
        Wrap(async () => new { isSuccess = true, result = await _svc.GetRecentBookingsAsync(_user.Id, ct) });

    [HttpGet("resources")]
    public Task<IActionResult> GetResources(CancellationToken ct) =>
        Wrap(async () =>
        {
            var r = await _svc.GetResourcesAsync(_user.Id, ct);
            return new { isSuccess = true, result = new { boats = r.Boats } };
        });

    [HttpPost("schedule")]
    public Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest req, CancellationToken ct) =>
        Wrap(async () =>
        {
            await _svc.CreateScheduleAsync(_user.Id, req, ct);
            return new { isSuccess = true, message = "Lịch trình đã được tạo!" };
        });

    [HttpPost("bookings/{id}/status")]
    public Task<IActionResult> UpdateBookingStatus(Guid id, [FromBody] UpdateBookingStatusRequest req, CancellationToken ct) =>
        Wrap(async () =>
        {
            var status = await _svc.UpdateBookingStatusAsync(_user.Id, id, req, ct);
            return new { isSuccess = true, message = $"Booking status updated to {status}" };
        });

    [HttpPost("clean-seed-data")]
    public Task<IActionResult> CleanSeedData(CancellationToken ct) =>
        Wrap(async () => new { isSuccess = true, message = await _svc.CleanSeedDataAsync(ct) });

    [HttpPost("rename-boats")]
    public Task<IActionResult> RenameBoats(CancellationToken ct) =>
        Wrap(async () => new { message = await _svc.RenameBoatsAsync(ct) });

    [HttpPost("seed-tours")]
    public Task<IActionResult> SeedTours(CancellationToken ct) =>
        Wrap(async () => new { isSuccess = true, message = await _svc.SeedToursAsync(ct) });

    private async Task<IActionResult> Wrap(Func<Task<object>> work)
    {
        try { return Ok(await work()); }
        catch (NotFoundException ex) { return NotFound(new { isSuccess = false, message = ex.Message }); }
        catch (AppException ex) { return BadRequest(new { isSuccess = false, message = ex.Message }); }
    }
}
