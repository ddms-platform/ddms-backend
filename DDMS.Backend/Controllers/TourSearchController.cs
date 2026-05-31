using DDMS.Backend.Common.Responses;

using DDMS.Backend.Models.DTOs.TourSearch;

using DDMS.Backend.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;



namespace DDMS.Backend.Controllers;



[ApiController]

[Route("api/tour-search")]

[Produces("application/json")]

[SwaggerTag("Search & Filter — location, price, date, duration, sort, remaining capacity")]

public class TourSearchController : ControllerBase

{

    private readonly ITourSearchService _service;



    public TourSearchController(ITourSearchService service)

    {

        _service = service;

    }



    [HttpGet]

    [SwaggerOperation(

        Summary = "Search available tour schedules",

        Description = "Filter by location, price range, date, status, duration. Sort by price or rating. Returns remaining_capacity per slot.")]

    [ProducesResponseType(typeof(ApiResponse<List<TourSearchResponse>>), StatusCodes.Status200OK)]

    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> Search([FromQuery] TourSearchRequest request, CancellationToken cancellationToken)

    {

        var data = await _service.SearchAsync(request, cancellationToken);

        return Ok(ApiResponse<List<TourSearchResponse>>.Ok(data));

    }

}


