using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/owner/services")]
// [Authorize(Roles = "owner")] — đang tắt để dễ test
public class OwnerServicesController : ControllerBase
{
    private readonly IOwnerServicesRegistrationService _registration;
    private readonly ICloudinaryService _cloudinary;

    public OwnerServicesController(
        IOwnerServicesRegistrationService registration,
        ICloudinaryService cloudinary)
    {
        _registration = registration;
        _cloudinary = cloudinary;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterService([FromBody] DynamicServiceRequest request, CancellationToken ct)
    {
        var result = await _registration.RegisterAsync(request, ct);
        return Ok(ApiResponse<TourResponse>.Ok(result));
    }

    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required." });

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);

        try
        {
            buffer.Position = 0;
            var upload = await _cloudinary.UploadImageAsync(buffer, file.FileName);
            return Ok(ApiResponse<string>.Ok(upload.ImageUrl));
        }
        catch
        {
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "image/jpeg"
                : file.ContentType;
            var dataUrl = $"data:{contentType};base64,{Convert.ToBase64String(buffer.ToArray())}";
            return Ok(ApiResponse<string>.Ok(dataUrl));
        }
    }
}
