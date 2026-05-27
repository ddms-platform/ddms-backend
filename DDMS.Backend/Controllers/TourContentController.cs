using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.TourContent;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tour-content")]
public class TourContentController : ControllerBase
{
    private readonly ITourContentService _service;

    public TourContentController(ITourContentService service)
    {
        _service = service;
    }

    [HttpPost("images")]
    public async Task<IActionResult> CreateImage([FromBody] CreateTourImageRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateImageAsync(request, cancellationToken);
        return Ok(ApiResponse<TourImageResponse>.Ok(data, MessageConstants.TOUR_IMAGE_CREATED));
    }

    [HttpPut("images/{id:guid}")]
    public async Task<IActionResult> UpdateImage(Guid id, [FromBody] UpdateTourImageRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateImageAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TourImageResponse>.Ok(data, MessageConstants.TOUR_IMAGE_UPDATED));
    }

    [HttpDelete("images/{id:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteImageAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, MessageConstants.TOUR_IMAGE_DELETED));
    }

    [HttpGet("images/tour/{tourId:guid}")]
    public async Task<IActionResult> GetImagesByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetImagesByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<TourImageResponse>>.Ok(data, MessageConstants.TOUR_IMAGES_FETCHED));
    }

    [HttpPost("faqs")]
    public async Task<IActionResult> CreateFaq([FromBody] CreateFaqRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateFaqAsync(request, cancellationToken);
        return Ok(ApiResponse<FaqResponse>.Ok(data, MessageConstants.FAQ_CREATED));
    }

    [HttpPut("faqs/{id:guid}")]
    public async Task<IActionResult> UpdateFaq(Guid id, [FromBody] UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateFaqAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FaqResponse>.Ok(data, MessageConstants.FAQ_UPDATED));
    }

    [HttpDelete("faqs/{id:guid}")]
    public async Task<IActionResult> DeleteFaq(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteFaqAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, MessageConstants.FAQ_DELETED));
    }

    [HttpGet("faqs/tour/{tourId:guid}")]
    public async Task<IActionResult> GetFaqByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetFaqsByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<FaqResponse>>.Ok(data, MessageConstants.FAQS_FETCHED));
    }

    [HttpPost("dock-schedules")]
    public async Task<IActionResult> CreateDockSchedule([FromBody] CreateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateDockScheduleAsync(request, cancellationToken);
        return Ok(ApiResponse<DockScheduleResponse>.Ok(data, MessageConstants.DOCK_SCHEDULE_CREATED));
    }

    [HttpPut("dock-schedules/{id:guid}")]
    public async Task<IActionResult> UpdateDockSchedule(Guid id, [FromBody] UpdateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateDockScheduleAsync(id, request, cancellationToken);
        return Ok(ApiResponse<DockScheduleResponse>.Ok(data, MessageConstants.DOCK_SCHEDULE_UPDATED));
    }

    [HttpDelete("dock-schedules/{id:guid}")]
    public async Task<IActionResult> DeleteDockSchedule(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteDockScheduleAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, MessageConstants.DOCK_SCHEDULE_DELETED));
    }

    [HttpGet("dock-schedules/dock/{dockId:guid}")]
    public async Task<IActionResult> GetDockSchedulesByDockId(Guid dockId, CancellationToken cancellationToken)
    {
        var data = await _service.GetDockSchedulesByDockIdAsync(dockId, cancellationToken);
        return Ok(ApiResponse<List<DockScheduleResponse>>.Ok(data, MessageConstants.DOCK_SCHEDULES_FETCHED));
    }
}
