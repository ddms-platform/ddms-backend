using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.AdminMaintenances;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/maintenances")]
public class AdminMaintenancesController : ControllerBase
{
    private readonly IAdminMaintenancesService _svc;
    public AdminMaintenancesController(IAdminMaintenancesService svc) => _svc = svc;

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct) =>
        Ok(ApiResponse<List<MaintenanceItem>>.Ok(await _svc.GetPendingAsync(ct)));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<List<MaintenanceItem>>.Ok(await _svc.GetAllAsync(ct)));

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
