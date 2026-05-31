using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Boat;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/boats/{boatId:guid}/images")]
public class BoatImagesController : ControllerBase
{
    private readonly IBoatImageService _imageService;

    public BoatImagesController(IBoatImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetImages(Guid boatId)
    {
        var result = await _imageService.GetByBoatIdAsync(boatId);
        return Ok(ApiResponse<List<BoatImageResponse>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Upload(Guid boatId, [FromBody] UploadBoatImageRequest request)
    {
        var result = await _imageService.UploadAsync(boatId, request.fileBase64, request.caption);
        return Ok(ApiResponse<BoatImageResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boatId, Guid id)
    {
        await _imageService.DeleteAsync(boatId, id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
