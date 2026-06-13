using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/boats/{boatId:guid}/cabins")]
public class BoatCabinsController : ControllerBase
{
    private readonly IBoatCabinService _cabinService;

    public BoatCabinsController(IBoatCabinService cabinService)
    {
        _cabinService = cabinService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCabins(Guid boatId)
    {
        var result = await _cabinService.GetByBoatIdAsync(boatId);
        return Ok(ApiResponse<List<BoatCabinResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid boatId, Guid id)
    {
        var result = await _cabinService.GetByIdAsync(boatId, id);
        return Ok(ApiResponse<BoatCabinResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid boatId, [FromBody] CreateBoatCabinRequest request)
    {
        var result = await _cabinService.CreateAsync(boatId, request);
        return Ok(ApiResponse<BoatCabinResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid boatId, Guid id, [FromBody] UpdateBoatCabinRequest request)
    {
        var result = await _cabinService.UpdateAsync(boatId, id, request);
        return Ok(ApiResponse<BoatCabinResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boatId, Guid id)
    {
        await _cabinService.DeleteAsync(boatId, id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
