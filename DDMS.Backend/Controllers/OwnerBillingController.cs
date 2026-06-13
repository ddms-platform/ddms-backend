using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Billing;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/owner/billing")]
public class OwnerBillingController : ControllerBase
{
    private readonly IBillingService _billing;
    private readonly ICurrentUser _user;

    public OwnerBillingController(IBillingService billing, ICurrentUser user)
    {
        _billing = billing;
        _user = user;
    }

    [Authorize(Roles = "owner")]
    [HttpGet("financial-summary")]
    public async Task<IActionResult> GetFinancialSummary(CancellationToken ct)
    {
        var result = await _billing.GetFinancialSummaryAsync(_user.Id, ct);
        return Ok(ApiResponse<FinancialSummaryResponse>.Ok(result));
    }

    [Authorize(Roles = "owner")]
    [HttpPost("pay")]
    public async Task<IActionResult> InitiatePayment(CancellationToken ct)
    {
        var result = await _billing.InitiatePaymentAsync(_user.Id, ct);
        return Ok(ApiResponse<PaymentInitResult>.Ok(result));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandlePayOSWebhook([FromBody] Webhook body, CancellationToken ct)
    {
        var r = await _billing.HandlePayOSWebhookAsync(body, ct);
        var payload = new { code = r.Code, desc = r.Desc };
        return r.IsSuccess ? Ok(payload) : BadRequest(payload);
    }
}
