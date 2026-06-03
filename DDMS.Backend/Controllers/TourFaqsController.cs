using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/tours/{tourId:guid}/faqs")]
public class TourFaqsController : ControllerBase
{
    private readonly IFaqService _faqService;

    public TourFaqsController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaqs(Guid tourId)
    {
        var result = await _faqService.GetByTourIdAsync(tourId, GetCurrentUserId());
        return Ok(ApiResponse<List<FaqItemResponse>>.Ok(result));
    }

    [HttpGet("{faqId:guid}")]
    public async Task<IActionResult> GetById(Guid tourId, Guid faqId)
    {
        var result = await _faqService.GetByIdAsync(tourId, faqId, GetCurrentUserId());
        return Ok(ApiResponse<FaqItemResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid tourId, [FromBody] CreateFaqRequest request)
    {
        var result = await _faqService.CreateAsync(tourId, GetCurrentUserId(), request);
        return Ok(ApiResponse<FaqItemResponse>.Ok(result));
    }

    [HttpPut("{faqId:guid}")]
    public async Task<IActionResult> Update(Guid tourId, Guid faqId, [FromBody] UpdateFaqRequest request)
    {
        var result = await _faqService.UpdateAsync(tourId, faqId, GetCurrentUserId(), request);
        return Ok(ApiResponse<FaqItemResponse>.Ok(result));
    }

    [HttpDelete("{faqId:guid}")]
    public async Task<IActionResult> Delete(Guid tourId, Guid faqId)
    {
        await _faqService.DeleteAsync(tourId, faqId, GetCurrentUserId());
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException();
        }

        return userId;
    }
}
