using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/boats/{boatId:guid}/services")]
public class BoatAddonsController : ControllerBase
{
    private readonly IBoatAddonService _addonService;

    public BoatAddonsController(IBoatAddonService addonService)
    {
        _addonService = addonService;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices(Guid boatId)
    {
        var result = await _addonService.GetByBoatIdAsync(boatId);
        return Ok(ApiResponse<List<BoatServiceResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid boatId, Guid id)
    {
        var result = await _addonService.GetByIdAsync(boatId, id);
        return Ok(ApiResponse<BoatServiceResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid boatId, [FromBody] CreateBoatServiceRequest request)
    {
        var result = await _addonService.CreateAsync(boatId, request);
        return Ok(ApiResponse<BoatServiceResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid boatId, Guid id, [FromBody] UpdateBoatServiceRequest request)
    {
        var result = await _addonService.UpdateAsync(boatId, id, request);
        return Ok(ApiResponse<BoatServiceResponse>.Ok(result));
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid boatId, Guid id)
    {
        var result = await _addonService.ToggleActiveAsync(boatId, id);
        return Ok(ApiResponse<BoatServiceResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boatId, Guid id)
    {
        await _addonService.DeleteAsync(boatId, id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
