using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/tours/{tourId:guid}")]
public class TourContentController : ControllerBase
{
    private readonly ITourImageService _tourImageService;

    public TourContentController(ITourImageService tourImageService)
    {
        _tourImageService = tourImageService;
    }

    [HttpPut("content")]
    public async Task<IActionResult> UpdateContent(Guid tourId, [FromBody] UpdateTourContentRequest request)
    {
        var result = await _tourImageService.UpdateContentAsync(tourId, GetCurrentUserId(), request);
        return Ok(ApiResponse<TourItemResponse>.Ok(result));
    }

    [HttpGet("images")]
    public async Task<IActionResult> GetImages(Guid tourId)
    {
        var result = await _tourImageService.GetByTourIdAsync(tourId, GetCurrentUserId());
        return Ok(ApiResponse<List<TourImageItemResponse>>.Ok(result));
    }

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(Guid tourId, [FromForm] UploadTourImageRequest request)
    {
        var result = await _tourImageService.UploadAsync(tourId, GetCurrentUserId(), request);
        return Ok(ApiResponse<TourImageItemResponse>.Ok(result));
    }

    [HttpPut("images/{imageId:guid}")]
    public async Task<IActionResult> UpdateImage(Guid tourId, Guid imageId, [FromBody] UpdateTourImageRequest request)
    {
        var result = await _tourImageService.UpdateAsync(tourId, imageId, GetCurrentUserId(), request);
        return Ok(ApiResponse<TourImageItemResponse>.Ok(result));
    }

    [HttpDelete("images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid tourId, Guid imageId)
    {
        await _tourImageService.DeleteAsync(tourId, imageId, GetCurrentUserId());
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