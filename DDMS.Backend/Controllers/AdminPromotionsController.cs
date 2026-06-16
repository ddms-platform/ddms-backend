using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Promotions;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/promotions")]
public class AdminPromotionsController : ControllerBase
{
    private readonly IAdminPromotionsService _svc;
    private readonly ICurrentUser _user;

    public AdminPromotionsController(IAdminPromotionsService svc, ICurrentUser user)
    {
        _svc = svc;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<List<AdminPromotionItem>>.Ok(await _svc.GetAllAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest req, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(_user.Id, req, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true, id }));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreatePromotionRequest req, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, req, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _svc.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await _svc.ApproveAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        await _svc.RejectAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var isActive = await _svc.ToggleActiveAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true, isActive }));
    }
}
