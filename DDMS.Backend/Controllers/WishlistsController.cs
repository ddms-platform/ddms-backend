using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Wishlists;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WishlistsController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistsController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlists()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var tours = await _wishlistService.GetWishlistToursAsync(userId);
        return Ok(new { items = tours, totalCount = tours.Count });
    }
    
    [HttpGet("ids")]
    public async Task<IActionResult> GetWishlistedTourIds()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var ids = await _wishlistService.GetWishlistedTourIdsAsync(userId);
        return Ok(ids);
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleWishlist([FromBody] WishlistToggleRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var isAdded = await _wishlistService.ToggleWishlistAsync(userId, request);
        return Ok(new { isAdded });
    }
}
