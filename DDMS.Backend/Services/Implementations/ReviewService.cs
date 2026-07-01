using DDMS.Backend.Models.DTOs.Review;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDMS.Backend.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public ReviewService(IReviewRepository reviewRepository, ICloudinaryService cloudinaryService)
        {
            _reviewRepository = reviewRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PaginatedReviewResult> GetReviewsByTourIdAsync(Guid tourId, int pageIndex, int pageSize)
        {
            var (reviews, totalCount) = await _reviewRepository.GetReviewsByTourIdAsync(tourId, pageIndex, pageSize);

            var dtos = reviews.Select(r => new ReviewDto
            {
                Id = r.id,
                UserId = r.user_id,
                UserName = r.user?.full_name ?? "Unknown",
                UserAvatarUrl = r.user?.avatar_url,
                TourId = r.tour_id,
                BookingId = r.booking_id,
                Rating = r.rating,
                Comment = r.comment,
                ImageUrls = string.IsNullOrEmpty(r.image_urls) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.image_urls),
                VideoUrls = string.IsNullOrEmpty(r.video_urls) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.video_urls),
                CreatedAt = r.created_at,
                UpdatedAt = r.updated_at
            }).ToList();

            return new PaginatedReviewResult
            {
                Reviews = dtos,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<List<Guid>> GetUnreviewedBookingIdsAsync(Guid userId, Guid tourId)
        {
            var bookings = await _reviewRepository.GetUnreviewedCompletedBookingsAsync(userId, tourId);
            return bookings.Select(b => b.id).ToList();
        }

        public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto)
        {
            // Verify if user already reviewed this booking
            if (await _reviewRepository.HasUserReviewedBookingAsync(dto.BookingId))
            {
                throw new InvalidOperationException("You have already reviewed this booking.");
            }

            var imageUrls = new List<string>();
            if (dto.Images != null)
            {
                foreach (var img in dto.Images)
                {
                    using var stream = img.OpenReadStream();
                    var result = await _cloudinaryService.UploadImageAsync(stream, img.FileName);
                    if (result != null && !string.IsNullOrEmpty(result.ImageUrl)) imageUrls.Add(result.ImageUrl);
                }
            }

            var videoUrls = new List<string>();
            if (dto.Videos != null)
            {
                foreach (var vid in dto.Videos)
                {
                    using var stream = vid.OpenReadStream();
                    var result = await _cloudinaryService.UploadVideoAsync(stream, vid.FileName);
                    if (result != null && !string.IsNullOrEmpty(result.ImageUrl)) videoUrls.Add(result.ImageUrl);
                }
            }

            var review = new review
            {
                id = Guid.NewGuid(),
                user_id = userId,
                booking_id = dto.BookingId,
                tour_id = dto.TourId,
                rating = (sbyte)dto.Rating,
                comment = dto.Comment,
                image_urls = imageUrls.Count > 0 ? JsonSerializer.Serialize(imageUrls) : null,
                video_urls = videoUrls.Count > 0 ? JsonSerializer.Serialize(videoUrls) : null,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await _reviewRepository.AddReviewAsync(review);

            return new ReviewDto { Id = review.id };
        }

        public async Task<ReviewDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null) throw new KeyNotFoundException("Review not found.");
            if (review.user_id != userId) throw new UnauthorizedAccessException("You can only edit your own reviews.");

            var imageUrls = string.IsNullOrEmpty(dto.ExistingImageUrls) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(dto.ExistingImageUrls);
            if (dto.NewImages != null)
            {
                foreach (var img in dto.NewImages)
                {
                    using var stream = img.OpenReadStream();
                    var result = await _cloudinaryService.UploadImageAsync(stream, img.FileName);
                    if (result != null && !string.IsNullOrEmpty(result.ImageUrl)) imageUrls.Add(result.ImageUrl);
                }
            }

            var videoUrls = string.IsNullOrEmpty(dto.ExistingVideoUrls) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(dto.ExistingVideoUrls);
            if (dto.NewVideos != null)
            {
                foreach (var vid in dto.NewVideos)
                {
                    using var stream = vid.OpenReadStream();
                    var result = await _cloudinaryService.UploadVideoAsync(stream, vid.FileName);
                    if (result != null && !string.IsNullOrEmpty(result.ImageUrl)) videoUrls.Add(result.ImageUrl);
                }
            }

            review.rating = (sbyte)dto.Rating;
            review.comment = dto.Comment;
            review.image_urls = imageUrls.Count > 0 ? JsonSerializer.Serialize(imageUrls) : null;
            review.video_urls = videoUrls.Count > 0 ? JsonSerializer.Serialize(videoUrls) : null;
            review.updated_at = DateTime.UtcNow;

            await _reviewRepository.UpdateReviewAsync(review);

            return new ReviewDto { Id = review.id };
        }

        public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null) return false;
            if (review.user_id != userId) throw new UnauthorizedAccessException("You can only delete your own reviews.");

            // Optionally delete from Cloudinary here (skipping for brevity but could extract publicIds from URLs)

            return await _reviewRepository.DeleteReviewAsync(reviewId);
        }
    }
}
