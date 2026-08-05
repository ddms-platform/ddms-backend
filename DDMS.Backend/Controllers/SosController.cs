using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Sos;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Emergency Real-time SOS Management")]
public class SosController : ControllerBase
{
    private readonly ISosService _sosService;

    public SosController(ISosService sosService)
    {
        _sosService = sosService;
    }

    [HttpPost("trigger")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Trigger real-time SOS alert", Description = "Sends captain GPS coordinates and broadcasts emergency alert to Port Authority Admins via SignalR.")]
    [ProducesResponseType(typeof(ApiResponse<SosAlertResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Trigger([FromBody] TriggerSosRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            // Fallback: System Captain ID for emergency signals when token/session is expired
            userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        var result = await _sosService.TriggerSosAsync(userId, request, ct);
        return Ok(ApiResponse<SosAlertResponse>.Ok(result));
    }

    [HttpGet("active")]
    [SwaggerOperation(Summary = "Get active SOS alerts", Description = "Returns all active emergency alerts for Port Authority Dashboard.")]
    [ProducesResponseType(typeof(ApiResponse<List<SosAlertResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAlerts(CancellationToken ct)
    {
        var result = await _sosService.GetActiveAlertsAsync(ct);
        return Ok(ApiResponse<List<SosAlertResponse>>.Ok(result));
    }

    [HttpGet("all")]
    [SwaggerOperation(Summary = "Get all SOS alerts with pagination", Description = "Returns paged emergency alerts history for Port Authority Center.")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SosAlertResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAlerts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        var result = await _sosService.GetPagedAlertsAsync(page, pageSize, status, ct);
        return Ok(ApiResponse<PagedResponse<SosAlertResponse>>.Ok(result));
    }

    [HttpPut("{id}/resolve")]
    [SwaggerOperation(Summary = "Resolve SOS alert", Description = "Marks an active SOS alert as resolved by Admin.")]
    [ProducesResponseType(typeof(ApiResponse<SosAlertResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveSosRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        var result = await _sosService.ResolveSosAsync(id, userId, request, ct);
        return Ok(ApiResponse<SosAlertResponse>.Ok(result));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete SOS alert record", Description = "Deletes an SOS alert record from the database.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sosService.DeleteSosAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Xóa bản ghi SOS thành công"));
    }
}
