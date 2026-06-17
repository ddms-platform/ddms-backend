using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.AdminOwners;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/owners")]
public class AdminOwnersController : ControllerBase
{
    private readonly IAdminOwnersService _svc;
    public AdminOwnersController(IAdminOwnersService svc) => _svc = svc;

    [HttpGet("verifications")]
    public async Task<IActionResult> GetVerifications(CancellationToken ct) =>
        Ok(ApiResponse<List<VerificationItem>>.Ok(await _svc.GetVerificationsAsync(ct)));

    [HttpPost("verifications/{id:guid}/approve")]
    public async Task<IActionResult> ApproveVerification(Guid id, CancellationToken ct)
    {
        var msg = await _svc.ApproveVerificationAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = msg }));
    }

    [HttpPost("verifications/{id:guid}/reject")]
    public async Task<IActionResult> RejectVerification(Guid id, CancellationToken ct)
    {
        var msg = await _svc.RejectVerificationAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = msg }));
    }
}
