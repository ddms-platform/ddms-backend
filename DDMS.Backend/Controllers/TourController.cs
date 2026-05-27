using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tour;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tours")]
public class TourController : ControllerBase
{
    private readonly ITourService _tourService;

    public TourController(ITourService tourService)
    {
        _tourService = tourService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTourRequest request, CancellationToken cancellationToken)
    {
        var data = await _tourService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<TourResponse>.Ok(data, MessageConstants.TOUR_CREATED));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourRequest request, CancellationToken cancellationToken)
    {
        var data = await _tourService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TourResponse>.Ok(data, MessageConstants.TOUR_UPDATED));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _tourService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, MessageConstants.TOUR_DELETED));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _tourService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TourResponse>.Ok(data, MessageConstants.TOUR_FETCHED));
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? status, [FromQuery] string? location, CancellationToken cancellationToken)
    {
        var data = await _tourService.GetListAsync(new TourFilterRequest
        {
            status = status,
            location = location
        }, cancellationToken);

        return Ok(ApiResponse<List<TourResponse>>.Ok(data, MessageConstants.TOURS_FETCHED));
    }
}
