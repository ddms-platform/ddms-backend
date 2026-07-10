using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.DTOs.OwnerBoats;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/owner/boats")]
public class OwnerBoatsController : ControllerBase
{
    private readonly IBoatService _boats;
    private readonly IBoatImageService _images;
    private readonly IBoatMaintenanceService _maintenances;
    private readonly IBoatCertificateService _certificates;
    private readonly ICurrentUser _user;

    public OwnerBoatsController(
        IBoatService boats,
        IBoatImageService images,
        IBoatMaintenanceService maintenances,
        IBoatCertificateService certificates,
        ICurrentUser user)
    {
        _boats = boats;
        _images = images;
        _maintenances = maintenances;
        _certificates = certificates;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetBoats([FromQuery] OwnerBoatListQuery query) =>
        Ok(ApiResponse<PagedResponse<BoatListItemResponse>>.Ok(await _boats.GetBoatsByOwnerAsync(_user.Id, query)));

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() =>
        Ok(ApiResponse<BoatStatsResponse>.Ok(await _boats.GetStatsByOwnerAsync(_user.Id)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(ApiResponse<BoatDetailResponse>.Ok(await _boats.GetByIdByOwnerAsync(id, _user.Id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoatRequest request) =>
        Ok(ApiResponse<BoatDetailResponse>.Ok(await _boats.CreateByOwnerAsync(request, _user.Id)));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoatRequest request) =>
        Ok(ApiResponse<BoatDetailResponse>.Ok(await _boats.UpdateByOwnerAsync(id, request, _user.Id)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _boats.DeleteByOwnerAsync(id, _user.Id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id)
    {
        await EnsureOwnedAsync(id);
        return Ok(ApiResponse<List<BoatImageResponse>>.Ok(await _images.GetByBoatIdAsync(id)));
    }

    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> UploadImage(Guid id, [FromBody] UploadBoatImageRequest request)
    {
        await EnsureOwnedAsync(id);
        return Ok(ApiResponse<BoatImageResponse>.Ok(await _images.UploadAsync(id, request.fileBase64, request.caption)));
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        await EnsureOwnedAsync(id);
        await _images.DeleteAsync(id, imageId);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    [HttpPost("{id:guid}/maintenances/register")]
    public async Task<IActionResult> RegisterMaintenances(Guid id,
        [FromBody] List<MaintenanceRegistrationRequest> registrations, CancellationToken ct)
    {
        var count = await _maintenances.RegisterAsync(id, _user.Id, registrations, ct);
        return Ok(ApiResponse<object>.Ok(new { success = true, registeredCount = count }));
    }

    [HttpDelete("{id:guid}/maintenances/{maintenanceId:guid}")]
    public async Task<IActionResult> DeleteMaintenance(Guid id, Guid maintenanceId, CancellationToken ct)
    {
        await _maintenances.DeleteAsync(id, maintenanceId, _user.Id, ct);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    [HttpGet("{id:guid}/certificates")]
    public async Task<IActionResult> GetCertificates(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<List<CertificateResponse>>.Ok(
            await _certificates.GetByBoatIdForOwnerAsync(id, _user.Id, ct)));

    [HttpPost("{id:guid}/certificates")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCertificate(
        Guid id, [FromForm] UploadCertificateRequest request, CancellationToken ct) =>
        Ok(ApiResponse<CertificateResponse>.Ok(
            await _certificates.UploadAsync(id, _user.Id, request, ct)));

    [HttpPost("{id:guid}/certificates/{certId:guid}/renew")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RenewCertificate(
        Guid id, Guid certId, [FromForm] RenewCertificateRequest request, CancellationToken ct) =>
        Ok(ApiResponse<CertificateResponse>.Ok(
            await _certificates.RenewAsync(id, certId, _user.Id, request, ct)));

    private async Task EnsureOwnedAsync(Guid boatId)
    {
        var boat = await _boats.GetByIdByOwnerAsync(boatId, _user.Id);
        if (boat == null) throw new Common.Exceptions.NotFoundException();
    }
}
