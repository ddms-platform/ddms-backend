using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.TourContent;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tour-content")]
[Produces("application/json")]
[SwaggerTag("Content Management — tour images (Cloudinary), FAQs, dock schedules")]
public class TourContentController : ControllerBase
{
    private readonly ITourContentService _service;

    public TourContentController(ITourContentService service)
    {
        _service = service;
    }

    [HttpPost("images")]
    [SwaggerOperation(
        Summary = "Add tour image",
        Description = "Stores image_url and public_id from Cloudinary upload. Supports caption and sort_order.")]
    [ProducesResponseType(typeof(ApiResponse<TourImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateImage([FromBody] CreateTourImageRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateImageAsync(request, cancellationToken);
        return Ok(ApiResponse<TourImageResponse>.Ok(data));
    }

    [HttpPut("images/{id:guid}")]
    [SwaggerOperation(Summary = "Update tour image", Description = "Updates image metadata, caption and gallery sort_order.")]
    [ProducesResponseType(typeof(ApiResponse<TourImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateImage(Guid id, [FromBody] UpdateTourImageRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateImageAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TourImageResponse>.Ok(data));
    }

    [HttpDelete("images/{id:guid}")]
    [SwaggerOperation(Summary = "Delete tour image", Description = "Removes an image record from tour gallery.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteImageAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("images/tour/{tourId:guid}")]
    [SwaggerOperation(Summary = "Get tour images", Description = "Returns gallery images ordered by sort_order.")]
    [ProducesResponseType(typeof(ApiResponse<List<TourImageResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImagesByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetImagesByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<TourImageResponse>>.Ok(data));
    }

    [HttpPost("faqs")]
    [SwaggerOperation(Summary = "Create FAQ", Description = "Adds question/answer for a tour with sort_order.")]
    [ProducesResponseType(typeof(ApiResponse<FaqResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFaq([FromBody] CreateFaqRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateFaqAsync(request, cancellationToken);
        return Ok(ApiResponse<FaqResponse>.Ok(data));
    }

    [HttpPut("faqs/{id:guid}")]
    [SwaggerOperation(Summary = "Update FAQ", Description = "Updates FAQ question, answer and sort_order.")]
    [ProducesResponseType(typeof(ApiResponse<FaqResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFaq(Guid id, [FromBody] UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateFaqAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FaqResponse>.Ok(data));
    }

    [HttpDelete("faqs/{id:guid}")]
    [SwaggerOperation(Summary = "Delete FAQ", Description = "Removes an FAQ entry by id.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFaq(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteFaqAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("faqs/tour/{tourId:guid}")]
    [SwaggerOperation(Summary = "Get FAQs by tour", Description = "Returns FAQs ordered by sort_order.")]
    [ProducesResponseType(typeof(ApiResponse<List<FaqResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFaqByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetFaqsByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<FaqResponse>>.Ok(data));
    }

    [HttpPost("dock-schedules")]
    [SwaggerOperation(
        Summary = "Assign boat to dock slot",
        Description = "Creates dock schedule. Validates time range and prevents overlapping slots on the same dock.")]
    [ProducesResponseType(typeof(ApiResponse<DockScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDockSchedule([FromBody] CreateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateDockScheduleAsync(request, cancellationToken);
        return Ok(ApiResponse<DockScheduleResponse>.Ok(data));
    }

    [HttpPut("dock-schedules/{id:guid}")]
    [SwaggerOperation(Summary = "Update dock schedule", Description = "Updates dock/boat assignment and time slot.")]
    [ProducesResponseType(typeof(ApiResponse<DockScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDockSchedule(Guid id, [FromBody] UpdateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateDockScheduleAsync(id, request, cancellationToken);
        return Ok(ApiResponse<DockScheduleResponse>.Ok(data));
    }

    [HttpDelete("dock-schedules/{id:guid}")]
    [SwaggerOperation(Summary = "Delete dock schedule", Description = "Removes a dock time slot.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDockSchedule(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteDockScheduleAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("dock-schedules/dock/{dockId:guid}")]
    [SwaggerOperation(Summary = "List dock schedules", Description = "Returns all boat assignments for a dock ordered by start_time.")]
    [ProducesResponseType(typeof(ApiResponse<List<DockScheduleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDockSchedulesByDockId(Guid dockId, CancellationToken cancellationToken)
    {
        var data = await _service.GetDockSchedulesByDockIdAsync(dockId, cancellationToken);
        return Ok(ApiResponse<List<DockScheduleResponse>>.Ok(data));
    }
}
