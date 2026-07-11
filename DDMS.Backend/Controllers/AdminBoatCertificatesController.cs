using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin")]
public class AdminBoatCertificatesController : ControllerBase
{
    private readonly IBoatCertificateService _certificates;
    private readonly ICurrentUser _user;

    public AdminBoatCertificatesController(IBoatCertificateService certificates, ICurrentUser user)
    {
        _certificates = certificates;
        _user = user;
    }

    [HttpGet("certificates/pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct) =>
        Ok(ApiResponse<List<CertificateListItem>>.Ok(await _certificates.GetPendingForAdminAsync(ct)));

    [HttpGet("certificates/approved")]
    public async Task<IActionResult> GetApproved(CancellationToken ct) =>
        Ok(ApiResponse<List<CertificateListItem>>.Ok(await _certificates.GetApprovedForAdminAsync(ct)));

    [HttpGet("certificates/expiring")]
    public async Task<IActionResult> GetExpiring(CancellationToken ct) =>
        Ok(ApiResponse<List<CertificateListItem>>.Ok(await _certificates.GetExpiringForAdminAsync(ct)));

    [HttpPost("certificates/{certId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid certId, CancellationToken ct)
    {
        await _certificates.ApproveAsync(certId, _user.Id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("certificates/{certId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid certId, [FromBody] RejectCertificateRequest request, CancellationToken ct)
    {
        await _certificates.RejectAsync(certId, _user.Id, request, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }

    [HttpPost("boats/{boatId:guid}/unlock")]
    public async Task<IActionResult> UnlockBoat(Guid boatId, CancellationToken ct)
    {
        await _certificates.UnlockBoatAsync(boatId, _user.Id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }
}
