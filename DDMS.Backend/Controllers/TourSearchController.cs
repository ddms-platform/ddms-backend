using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tours/search")]
public class TourSearchController : ControllerBase
{
    private readonly ITourSearchService _tourSearchService;

    public TourSearchController(ITourSearchService tourSearchService)
    {
        _tourSearchService = tourSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] TourSearchQuery query)
    {
        var result = await _tourSearchService.SearchAsync(query);
        return Ok(ApiResponse<PagedResponse<TourSearchItemResponse>>.Ok(result));
    }
}