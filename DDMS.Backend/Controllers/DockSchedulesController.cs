using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/dock-schedules")]
public class DockSchedulesController : ControllerBase
{
    private readonly IDockScheduleService _dockScheduleService;

    public DockSchedulesController(IDockScheduleService dockScheduleService)
    {
        _dockScheduleService = dockScheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules([FromQuery] DockScheduleListQuery query)
    {
        var result = await _dockScheduleService.GetSchedulesAsync(query);
        return Ok(ApiResponse<PagedResponse<DockScheduleItemResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _dockScheduleService.GetByIdAsync(id);
        return Ok(ApiResponse<DockScheduleItemResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDockScheduleRequest request)
    {
        var result = await _dockScheduleService.CreateAsync(request);
        return Ok(ApiResponse<DockScheduleItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDockScheduleRequest request)
    {
        var result = await _dockScheduleService.UpdateAsync(id, request);
        return Ok(ApiResponse<DockScheduleItemResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _dockScheduleService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
