using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.PublicOwners;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/public/owners")]
public class PublicOwnersController : ControllerBase
{
    private readonly IPublicOwnersService _owners;

    public PublicOwnersController(IPublicOwnersService owners) => _owners = owners;

    /// <summary>Chủ thuyền đã được cảng vụ xác thực, dùng cho khối đối tác ở trang chủ.</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(
        [FromQuery] int take = 3, CancellationToken ct = default)
    {
        var limit = Math.Clamp(take, 1, 12);
        var result = await _owners.GetFeaturedAsync(limit, ct);
        return Ok(ApiResponse<List<FeaturedOwnerResponse>>.Ok(result));
    }
}
