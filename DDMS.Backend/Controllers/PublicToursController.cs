using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

/// <summary>Public read-only tour catalog for customers (search result detail).</summary>
[ApiController]
[Route("api/public/tours")]
public class PublicToursController : ControllerBase
{
    private readonly IPublicTourCatalogService _catalogService;

    public PublicToursController(IPublicTourCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("{tourId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TourItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid tourId)
    {
        var result = await _catalogService.GetActiveTourAsync(tourId);
        return Ok(ApiResponse<TourItemResponse>.Ok(result));
    }

    [HttpGet("{tourId:guid}/images")]
    [ProducesResponseType(typeof(ApiResponse<List<TourImageItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImages(Guid tourId)
    {
        var result = await _catalogService.GetTourImagesAsync(tourId);
        return Ok(ApiResponse<List<TourImageItemResponse>>.Ok(result));
    }

    [HttpGet("{tourId:guid}/faqs")]
    [ProducesResponseType(typeof(ApiResponse<List<FaqItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFaqs(Guid tourId)
    {
        var result = await _catalogService.GetTourFaqsAsync(tourId);
        return Ok(ApiResponse<List<FaqItemResponse>>.Ok(result));
    }
}
