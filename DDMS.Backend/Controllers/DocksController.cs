using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin,owner")]
[ApiController]
[Route("api/docks")]
[Route("api/admin/docks")]
public class DocksController : ControllerBase
{
    private readonly IDockService _dockService;
    private readonly IDockScheduleService _dockScheduleService;

    public DocksController(IDockService dockService, IDockScheduleService dockScheduleService)
    {
        _dockService = dockService;
        _dockScheduleService = dockScheduleService;
    }

    // ── Dock CRUD ─────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetDocks([FromQuery] DockListQuery query)
    {
        var result = await _dockService.GetDocksAsync(query);
        return Ok(ApiResponse<PagedResponse<DockListItemResponse>>.Ok(result));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dockService.GetStatsAsync();
        return Ok(ApiResponse<DockStatsResponse>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _dockService.GetAllAsync();
        return Ok(ApiResponse<List<DockListItemResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _dockService.GetByIdAsync(id);
        return Ok(ApiResponse<DockListItemResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDockRequest request)
    {
        var result = await _dockService.CreateAsync(request);
        return Ok(ApiResponse<DockListItemResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDockRequest request)
    {
        var result = await _dockService.UpdateAsync(id, request);
        return Ok(ApiResponse<DockListItemResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _dockService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    // ── Schedules ─────────────────────────────────────────────

    /// <summary>
    /// Cang vu gan khoang neo cho mot lich neo dau. Body rong hoac berthCode
    /// rong thi go khoang.
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPut("schedules/{dockScheduleId:guid}/berth")]
    public async Task<IActionResult> AssignBerth(
        Guid dockScheduleId, [FromBody] AssignBerthRequest request, CancellationToken ct)
    {
        var result = await _dockScheduleService.AssignBerthAsync(dockScheduleId, request?.berthCode, ct);
        return Ok(ApiResponse<BerthAssignmentResponse>.Ok(result));
    }

    /// <summary>Danh sach khoang cua ben, dung de admin chon.</summary>
    [HttpGet("{dockId:guid}/berths")]
    public async Task<IActionResult> GetBerths(Guid dockId)
    {
        var dock = await _dockService.GetByIdAsync(dockId);
        var berths = DockBerths.Ordered.Take(dock.maxBoats).ToList();
        return Ok(ApiResponse<List<string>>.Ok(berths));
    }


    [HttpGet("{dockId:guid}/schedules")]
    public async Task<IActionResult> GetSchedules(Guid dockId)
    {
        var result = await _dockService.GetSchedulesAsync(dockId);
        return Ok(ApiResponse<List<DockScheduleResponse>>.Ok(result));
    }

    [HttpPost("{dockId:guid}/schedules")]
    public async Task<IActionResult> AddSchedule(Guid dockId, [FromBody] CreateDockScheduleRequest request)
    {
        var result = await _dockService.AddScheduleAsync(dockId, request);
        return Ok(ApiResponse<DockScheduleResponse>.Ok(result));
    }

    [HttpDelete("{dockId:guid}/schedules/{scheduleId:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid dockId, Guid scheduleId)
    {
        await _dockService.DeleteScheduleAsync(dockId, scheduleId);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
