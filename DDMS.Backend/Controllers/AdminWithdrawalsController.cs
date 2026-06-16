using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.AdminWithdrawals;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/withdrawals")]
public class AdminWithdrawalsController : ControllerBase
{
    private readonly IAdminWithdrawalsService _svc;
    public AdminWithdrawalsController(IAdminWithdrawalsService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAllWithdrawals(CancellationToken ct) =>
        Ok(ApiResponse<List<WithdrawalItem>>.Ok(await _svc.GetAllAsync(ct)));

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
}
