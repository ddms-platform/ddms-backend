using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
public class BoatsController : ControllerBase
{
    private readonly IBoatService _boatService;

    public BoatsController(IBoatService boatService)
    {
        _boatService = boatService;
    }

    // ── Admin endpoints ───────────────────────────────────────

    [Authorize(Roles = "admin")]
    [HttpGet("api/admin/boats")]
    public async Task<IActionResult> GetBoats([FromQuery] BoatListQuery query)
    {
        var result = await _boatService.GetBoatsAsync(query);
        return Ok(ApiResponse<PagedResponse<BoatListItemResponse>>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpGet("api/admin/boats/stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _boatService.GetStatsAsync();
        return Ok(ApiResponse<BoatStatsResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpGet("api/admin/boats/all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _boatService.GetAllAsync();
        return Ok(ApiResponse<List<BoatListItemResponse>>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpGet("api/admin/boats/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _boatService.GetByIdAsync(id);
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpPost("api/admin/boats")]
    public async Task<IActionResult> Create([FromBody] CreateBoatRequest request)
    {
        var result = await _boatService.CreateAsync(request);
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpPut("api/admin/boats/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoatRequest request)
    {
        var result = await _boatService.UpdateAsync(id, request);
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("api/admin/boats/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _boatService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    // ── Public endpoints (cho user frontend) ─────────────────

    [HttpGet("api/boats")]
    public async Task<IActionResult> GetBoatsPublic([FromQuery] BoatListQuery query)
    {
        // Chỉ hiện thuyền đang hoạt động (running) cho public
        query.status = "running";
        var result = await _boatService.GetBoatsAsync(query);
        return Ok(ApiResponse<PagedResponse<BoatListItemResponse>>.Ok(result));
    }

    [HttpGet("api/boats/{id:guid}")]
    public async Task<IActionResult> GetByIdPublic(Guid id)
    {
        var result = await _boatService.GetByIdAsync(id);
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }
}
