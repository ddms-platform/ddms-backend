using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules([FromQuery] ScheduleListQuery query)
    {
        var result = await _scheduleService.GetSchedulesAsync(GetCurrentUserId(), query);
        return Ok(ApiResponse<PagedResponse<ScheduleItemResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _scheduleService.GetByIdAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<ScheduleItemResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScheduleRequest request)
    {
        var result = await _scheduleService.CreateAsync(GetCurrentUserId(), request);
        return Ok(ApiResponse<ScheduleItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        var result = await _scheduleService.UpdateAsync(id, GetCurrentUserId(), request);
        return Ok(ApiResponse<ScheduleItemResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _scheduleService.DeleteAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException();
        }

        return userId;
    }
}
