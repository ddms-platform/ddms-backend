using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/owner/boats")]
public class OwnerBoatsController : ControllerBase
{
    private readonly IBoatService _boatService;

    public OwnerBoatsController(IBoatService boatService)
    {
        _boatService = boatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBoats([FromQuery] OwnerBoatListQuery query)
    {
        var result = await _boatService.GetBoatsByOwnerAsync(GetCurrentUserId(), query);
        return Ok(ApiResponse<PagedResponse<BoatListItemResponse>>.Ok(result));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _boatService.GetStatsByOwnerAsync(GetCurrentUserId());
        return Ok(ApiResponse<BoatStatsResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _boatService.GetByIdByOwnerAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoatRequest request)
    {
        var result = await _boatService.CreateByOwnerAsync(request, GetCurrentUserId());
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoatRequest request)
    {
        var result = await _boatService.UpdateByOwnerAsync(id, request, GetCurrentUserId());
        return Ok(ApiResponse<BoatDetailResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _boatService.DeleteByOwnerAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();
        return userId;
    }
}
