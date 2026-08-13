using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.AdminOps;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/ops")]
public class AdminOpsController : ControllerBase
{
    private readonly IAdminOpsService _ops;
    private readonly ICurrentUser _currentUser;

    public AdminOpsController(IAdminOpsService ops, ICurrentUser currentUser)
    {
        _ops = ops;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Morning ops briefing — aggregated signals + AI-generated narrative.
    /// </summary>
    [HttpGet("briefing")]
    public async Task<IActionResult> GetBriefing(CancellationToken ct)
    {
        var result = await _ops.GetMorningBriefingAsync(ct);
        return Ok(ApiResponse<OpsBriefingResponse>.Ok(result));
    }

    /// <summary>
    /// Admin natural-language query over system data.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Ask([FromBody] AdminOpsChatRequest request, CancellationToken ct)
    {
        var result = await _ops.AskAsync(_currentUser.Id, request, ct);
        return Ok(ApiResponse<AdminOpsChatResponse>.Ok(result));
    }

    /// <summary>What-if simulator — analyse impact of a hypothetical scenario.</summary>
    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] WhatIfSimRequest request, CancellationToken ct)
    {
        var result = await _ops.SimulateAsync(request, ct);
        return Ok(ApiResponse<WhatIfSimResponse>.Ok(result));
    }

    /// <summary>Streaming version — SSE.</summary>
    [HttpPost("chat/stream")]
    public async Task AskStream([FromBody] AdminOpsChatRequest request, CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        try
        {
            await foreach (var delta in _ops.AskStreamAsync(_currentUser.Id, request, ct))
            {
                var payload = System.Text.Json.JsonSerializer.Serialize(new { delta });
                var line = $"data: {payload}\n\n";
                var bytes = System.Text.Encoding.UTF8.GetBytes(line);
                await Response.Body.WriteAsync(bytes, ct);
                await Response.Body.FlushAsync(ct);
            }
            var done = System.Text.Encoding.UTF8.GetBytes("data: {\"done\":true}\n\n");
            await Response.Body.WriteAsync(done, ct);
            await Response.Body.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // client aborted
        }
    }
}
