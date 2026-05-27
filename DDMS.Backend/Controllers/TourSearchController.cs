using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.TourSearch;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tour-search")]
public class TourSearchController : ControllerBase
{
    private readonly ITourSearchService _service;

    public TourSearchController(ITourSearchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? location,
        [FromQuery] decimal? min_price,
        [FromQuery] decimal? max_price,
        [FromQuery] DateTime? date,
        [FromQuery] string? status,
        [FromQuery] int? min_duration_minutes,
        [FromQuery] int? max_duration_minutes,
        [FromQuery] string? sort_by,
        [FromQuery] bool sort_desc,
        CancellationToken cancellationToken)
    {
        var data = await _service.SearchAsync(new TourSearchRequest
        {
            location = location,
            min_price = min_price,
            max_price = max_price,
            date = date,
            status = status,
            min_duration_minutes = min_duration_minutes,
            max_duration_minutes = max_duration_minutes,
            sort_by = sort_by,
            sort_desc = sort_desc
        }, cancellationToken);

        return Ok(ApiResponse<List<TourSearchResponse>>.Ok(data, MessageConstants.SEARCH_RESULT_FETCHED));
    }
}
