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
[Route("api/tours")]
public class ToursController : ControllerBase
{
    private readonly ITourService _tourService;

    public ToursController(ITourService tourService)
    {
        _tourService = tourService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTours([FromQuery] TourListQuery query)
    {
        var result = await _tourService.GetToursAsync(GetCurrentUserId(), query);
        return Ok(ApiResponse<PagedResponse<TourItemResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _tourService.GetByIdAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<TourItemResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTourRequest request)
    {
        var result = await _tourService.CreateAsync(GetCurrentUserId(), request);
        return Ok(ApiResponse<TourItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourRequest request)
    {
        var result = await _tourService.UpdateAsync(id, GetCurrentUserId(), request);
        return Ok(ApiResponse<TourItemResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _tourService.DeleteAsync(id, GetCurrentUserId());
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
