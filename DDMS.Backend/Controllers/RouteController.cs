using DDMS.Backend.Common.Responses;

using DDMS.Backend.Models.DTOs.Route;

using DDMS.Backend.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;



namespace DDMS.Backend.Controllers;



[ApiController]

[Route("api/routes")]

[Produces("application/json")]

[SwaggerTag("Route CRUD — start/end points, sort_order, linked to tour")]

public class RouteController : ControllerBase

{

    private readonly IRouteService _service;



    public RouteController(IRouteService service)

    {

        _service = service;

    }



    [HttpPost]

    [SwaggerOperation(Summary = "Create route", Description = "Adds a route segment (start_point, end_point, sort_order) for a tour.")]

    [ProducesResponseType(typeof(ApiResponse<RouteResponse>), StatusCodes.Status200OK)]

    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request, CancellationToken cancellationToken)

    {

        var data = await _service.CreateAsync(request, cancellationToken);

        return Ok(ApiResponse<RouteResponse>.Ok(data));

    }



    [HttpPut("{id:guid}")]

    [SwaggerOperation(Summary = "Update route", Description = "Updates route points and sort order.")]

    [ProducesResponseType(typeof(ApiResponse<RouteResponse>), StatusCodes.Status200OK)]

    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]

    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRouteRequest request, CancellationToken cancellationToken)

    {

        var data = await _service.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse<RouteResponse>.Ok(data));

    }



    [HttpDelete("{id:guid}")]

    [SwaggerOperation(Summary = "Delete route", Description = "Removes a route by id.")]

    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]

    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)

    {

        await _service.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse<object>.Ok(null!));

    }



    [HttpGet("tour/{tourId:guid}")]

    [SwaggerOperation(Summary = "Get routes by tour", Description = "Returns all routes for a tour ordered by sort_order.")]

    [ProducesResponseType(typeof(ApiResponse<List<RouteResponse>>), StatusCodes.Status200OK)]

    public async Task<IActionResult> GetByTourId(Guid tourId, CancellationToken cancellationToken)

    {

        var data = await _service.GetByTourIdAsync(tourId, cancellationToken);

        return Ok(ApiResponse<List<RouteResponse>>.Ok(data));

    }

}


