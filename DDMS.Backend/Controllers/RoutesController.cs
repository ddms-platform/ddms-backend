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
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RoutesController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoutes([FromQuery] RouteListQuery query)
    {
        var result = await _routeService.GetRoutesAsync(GetCurrentUserId(), query);
        return Ok(ApiResponse<PagedResponse<RouteItemResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _routeService.GetByIdAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<RouteItemResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request)
    {
        var result = await _routeService.CreateAsync(GetCurrentUserId(), request);
        return Ok(ApiResponse<RouteItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRouteRequest request)
    {
        var result = await _routeService.UpdateAsync(id, GetCurrentUserId(), request);
        return Ok(ApiResponse<RouteItemResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _routeService.DeleteAsync(id, GetCurrentUserId());
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
