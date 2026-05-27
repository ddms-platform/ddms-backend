using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.TourSchedule;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tour-schedules")]
public class TourScheduleController : ControllerBase
{
    private readonly ITourScheduleService _service;

    public TourScheduleController(ITourScheduleService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<TourScheduleResponse>.Ok(data, MessageConstants.SCHEDULE_CREATED));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourScheduleRequest request, CancellationToken cancellationToken)
    {
        var data = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TourScheduleResponse>.Ok(data, MessageConstants.SCHEDULE_UPDATED));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, MessageConstants.SCHEDULE_DELETED));
    }

    [HttpGet("tour/{tourId:guid}")]
    public async Task<IActionResult> GetByTourId(Guid tourId, CancellationToken cancellationToken)
    {
        var data = await _service.GetByTourIdAsync(tourId, cancellationToken);
        return Ok(ApiResponse<List<TourScheduleResponse>>.Ok(data, MessageConstants.SCHEDULES_FETCHED));
    }
}
