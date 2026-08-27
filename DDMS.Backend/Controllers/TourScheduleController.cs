using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.TourSchedule;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tour-schedules")]
[Produces("application/json")]
[SwaggerTag("Schedule Management — add/edit schedules, assign boat and dock")]
public class TourScheduleController : ControllerBase
{
    private readonly ITourScheduleService _service;

    public TourScheduleController(ITourScheduleService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Owner},{RoleNames.Admin}")]
    [SwaggerOperation(
        Summary = "Create tour schedule",
        Description = "Add schedule with date/time, link to tour. Optionally assign boat and dock. Status: scheduled / ongoing / completed / cancelled.")]
    [ProducesResponseType(typeof(ApiResponse<TourScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<TourScheduleResponse>.Ok(data));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Owner},{RoleNames.Admin}")]
    [SwaggerOperation(Summary = "Update tour schedule", Description = "Edit schedule time, boat/dock assignment and status.")]
    [ProducesResponseType(typeof(ApiResponse<TourScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TourScheduleResponse>.Ok(data));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Owner},{RoleNames.Admin}")]
    [SwaggerOperation(Summary = "Delete tour schedule", Description = "Removes a schedule slot by id.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("tour/{tourId:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get schedules by tour", Description = "Returns all schedules for a tour ordered by start time.")]
    [ProducesResponseType(typeof(ApiResponse<List<TourScheduleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<TourScheduleResponse>>.Ok(data));
    }
}
