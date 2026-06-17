using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/tours/search")]
public class TourSearchController : ControllerBase
{
    private readonly IPublicTourSearchService _tourSearchService;

    public TourSearchController(IPublicTourSearchService tourSearchService)
    {
        _tourSearchService = tourSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] TourSearchQuery query)
    {
        var result = await _tourSearchService.SearchAsync(query);
        return Ok(ApiResponse<PagedResponse<TourSearchItemResponse>>.Ok(result));
    }

    [HttpGet("~/api/public/tours/destinations/popular")]
    public async Task<IActionResult> GetPopularDestinations([FromQuery] int limit = 3)
    {
        var result = await _tourSearchService.GetPopularDestinationsAsync(limit);
        return Ok(ApiResponse<List<PopularDestinationResponse>>.Ok(result));
    }
}
