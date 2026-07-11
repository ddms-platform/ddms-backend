using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/certificate-types")]
public class AdminCertificateTypesController : ControllerBase
{
    private readonly ICertificateTypeService _types;

    public AdminCertificateTypesController(ICertificateTypeService types) => _types = types;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<List<CertificateTypeItem>>.Ok(await _types.GetAllForAdminAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCertificateTypeRequest request, CancellationToken ct) =>
        Ok(ApiResponse<CertificateTypeItem>.Ok(await _types.CreateAsync(request, ct)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCertificateTypeRequest request, CancellationToken ct) =>
        Ok(ApiResponse<CertificateTypeItem>.Ok(await _types.UpdateAsync(id, request, ct)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _types.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }
}
