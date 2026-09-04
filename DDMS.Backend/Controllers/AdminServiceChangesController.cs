using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = RoleNames.Admin)]
[ApiController]
[Route("api/admin/service-changes")]
public class AdminServiceChangesController : ControllerBase
{
    private readonly IOwnerServicesRegistrationService _registration;

    public AdminServiceChangesController(IOwnerServicesRegistrationService registration)
    {
        _registration = registration;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    {
        var items = await _registration.ListChangesAsync(status, ct);
        return Ok(ApiResponse<List<ServiceChangeRequestResponse>>.Ok(items));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _registration.ApproveChangeAsync(id, ct);
        return Ok(ApiResponse<ServiceChangeRequestResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectServiceChangeRequest request,
        CancellationToken ct)
    {
        var result = await _registration.RejectChangeAsync(id, request.reason, ct);
        return Ok(ApiResponse<ServiceChangeRequestResponse>.Ok(result));
    }
}
