using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Route;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/routes")]
public class RouteController : ControllerBase
{
    private readonly IRouteService _service;

    public RouteController(IRouteService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<RouteResponse>.Ok(data, MessageConstants.ROUTE_CREATED));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRouteRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<RouteResponse>.Ok(data, MessageConstants.ROUTE_UPDATED));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, MessageConstants.ROUTE_DELETED));
    }

    [HttpGet("tour/{tourId:guid}")]
    public async Task<IActionResult> GetByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<RouteResponse>>.Ok(data, MessageConstants.ROUTES_FETCHED));
    }
}
