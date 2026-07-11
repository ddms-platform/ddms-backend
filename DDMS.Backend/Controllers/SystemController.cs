using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly ISystemService _system;
    private readonly ICertificateTypeService _certificateTypes;

    public SystemController(ISystemService system, ICertificateTypeService certificateTypes)
    {
        _system = system;
        _certificateTypes = certificateTypes;
    }

    [HttpGet("boat-types")]
    public async Task<IActionResult> GetBoatTypes(CancellationToken ct) =>
        Ok(await _system.GetBoatTypesAsync(ct));

    [HttpGet("certificate-types")]
    public async Task<IActionResult> GetCertificateTypes(CancellationToken ct) =>
        Ok(ApiResponse<List<CertificateTypeItem>>.Ok(await _certificateTypes.GetActiveAsync(ct)));
}
