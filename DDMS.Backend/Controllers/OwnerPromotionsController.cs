using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Promotions;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/owner/promotions")]
public class OwnerPromotionsController : ControllerBase
{
    private readonly IOwnerPromotionsService _svc;
    private readonly ICurrentUser _user;

    public OwnerPromotionsController(IOwnerPromotionsService svc, ICurrentUser user)
    {
        _svc = svc;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetOwnerPromotions(CancellationToken ct) =>
        Ok(ApiResponse<List<PromotionItem>>.Ok(await _svc.GetMineAsync(_user.Id, ct)));

    [HttpPost]
    public async Task<IActionResult> CreateOwnerPromotion([FromBody] CreatePromotionRequest req, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(_user.Id, req, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true, id }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteOwnerPromotion(Guid id, CancellationToken ct)
    {
        await _svc.DeleteAsync(id, _user.Id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }
}
