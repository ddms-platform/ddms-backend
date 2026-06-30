using DDMS.Backend.Common.Identity;
using DDMS.Backend.Models.DTOs.Review;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DDMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ICurrentUser _currentUser;

        public ReviewsController(IReviewService reviewService, ICurrentUser currentUser)
        {
            _reviewService = reviewService;
            _currentUser = currentUser;
        }

        [HttpGet("tour/{tourId}")]
        public async Task<IActionResult> GetReviewsByTourId(Guid tourId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _reviewService.GetReviewsByTourIdAsync(tourId, pageIndex, pageSize);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("can-review/{tourId}")]
        [Authorize]
        public async Task<IActionResult> CanReviewTour(Guid tourId)
        {
            var userId = _currentUser.Id;
            var bookingIds = await _reviewService.GetUnreviewedBookingIdsAsync(userId, tourId);
            return Ok(new { success = true, canReview = bookingIds.Count > 0, bookingIds });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromForm] CreateReviewDto dto)
        {
            var userId = _currentUser.Id;
            var result = await _reviewService.CreateReviewAsync(userId, dto);
            return Ok(new { success = true, data = result });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(Guid id, [FromForm] UpdateReviewDto dto)
        {
            var userId = _currentUser.Id;
            var result = await _reviewService.UpdateReviewAsync(userId, id, dto);
            return Ok(new { success = true, data = result });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userId = _currentUser.Id;
            var result = await _reviewService.DeleteReviewAsync(userId, id);
            return Ok(new { success = result });
        }
    }
}
