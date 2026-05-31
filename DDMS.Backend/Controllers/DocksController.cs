using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Dock;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/docks")]
public class DocksController : ControllerBase
{
    private readonly IDockService _dockService;

    public DocksController(IDockService dockService)
    {
        _dockService = dockService;
    }

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDockRequest request)
    {
        var result = await _dockService.CreateAsync(request);
        return Ok(ApiResponse<DockListItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDockRequest request)
    {
        var result = await _dockService.UpdateAsync(id, request);
        return Ok(ApiResponse<DockListItemResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _dockService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
