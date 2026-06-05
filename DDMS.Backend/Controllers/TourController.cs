using DDMS.Backend.Common.Responses;using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/legacy/tours")]
[Produces("application/json")]
[SwaggerTag("Tour Management — CRUD tours, filter by status/location, cancel policy config")]
public class TourController : ControllerBase
{
    private readonly ITourService _tourService;

    public TourController(ITourService tourService)
    {
        _tourService = tourService;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new tour",
        Description = "Creates tour with name, price, description, duration, location. Cancel policy: free / partial / no_refund. Optional cancel_hours.")]
    [ProducesResponseType(typeof(ApiResponse<TourResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTourRequest request, CancellationToken cancellationToken)
    {
        var data = await _tourService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<TourResponse>.Ok(data));
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update an existing tour", Description = "Updates tour fields including cancel policy and status.")]
    [ProducesResponseType(typeof(ApiResponse<TourResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourRequest request, CancellationToken cancellationToken)
    {
        var data = await _tourService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TourResponse>.Ok(data));
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a tour", Description = "Permanently removes a tour by id.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _tourService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get tour by id", Description = "Returns full tour detail including rating and cancel policy.")]
    [ProducesResponseType(typeof(ApiResponse<TourResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _tourService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TourResponse>.Ok(data));
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "List tours with filters",
        Description = "Filter by status (active/inactive) and location (partial match).")]
    [ProducesResponseType(typeof(ApiResponse<List<TourResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList([FromQuery] TourFilterRequest request, CancellationToken cancellationToken)
    {
        var data = await _tourService.GetListAsync(request, cancellationToken);
        return Ok(ApiResponse<List<TourResponse>>.Ok(data));
    }
}
