using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/tours/{tourId:guid}/faqs")]
public class TourFaqsController : ControllerBase
{
    private readonly IFaqService _faqService;
    private readonly ICurrentUser _user;

    public TourFaqsController(IFaqService faqService, ICurrentUser user)
    {
        _faqService = faqService;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetFaqs(Guid tourId)
    {
        var result = await _faqService.GetByTourIdAsync(tourId, _user.Id);
        return Ok(ApiResponse<List<FaqItemResponse>>.Ok(result));
    }

    [HttpGet("{faqId:guid}")]
    public async Task<IActionResult> GetById(Guid tourId, Guid faqId)
    {
        var result = await _faqService.GetByIdAsync(tourId, faqId, _user.Id);
        return Ok(ApiResponse<FaqItemResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid tourId, [FromBody] CreateFaqRequest request)
    {
        var result = await _faqService.CreateAsync(tourId, _user.Id, request);
        return Ok(ApiResponse<FaqItemResponse>.Ok(result));
    }

    [HttpPut("{faqId:guid}")]
    public async Task<IActionResult> Update(Guid tourId, Guid faqId, [FromBody] UpdateFaqRequest request)
    {
        var result = await _faqService.UpdateAsync(tourId, faqId, _user.Id, request);
        return Ok(ApiResponse<FaqItemResponse>.Ok(result));
    }

    [HttpDelete("{faqId:guid}")]
    public async Task<IActionResult> Delete(Guid tourId, Guid faqId)
    {
        await _faqService.DeleteAsync(tourId, faqId, _user.Id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
