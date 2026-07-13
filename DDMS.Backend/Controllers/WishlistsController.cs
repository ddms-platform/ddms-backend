using System;
using System.Threading.Tasks;
using DDMS.Backend.Common.Identity;
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
    private readonly ICurrentUser _user;

    public WishlistsController(IWishlistService wishlistService, ICurrentUser user)
    {
        _wishlistService = wishlistService;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlists()
    {
        var userId = _user.IdOrNull;
        if (userId is null)
        {
            return Unauthorized();
        }

        var tours = await _wishlistService.GetWishlistToursAsync(userId.Value);
        return Ok(new { items = tours, totalCount = tours.Count });
    }
    
    [HttpGet("ids")]
    public async Task<IActionResult> GetWishlistedTourIds()
    {
        var userId = _user.IdOrNull;
        if (userId is null)
        {
            return Unauthorized();
        }

        var ids = await _wishlistService.GetWishlistedTourIdsAsync(userId.Value);
        return Ok(ids);
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleWishlist([FromBody] WishlistToggleRequest request)
    {
        var userId = _user.IdOrNull;
        if (userId is null)
        {
            return Unauthorized();
        }

        var isAdded = await _wishlistService.ToggleWishlistAsync(userId.Value, request);
        return Ok(new { isAdded });
    }
}
