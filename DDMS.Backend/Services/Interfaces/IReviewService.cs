using DDMS.Backend.Models.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDMS.Backend.Services.Interfaces
{
    public interface IReviewService
    {
        Task<PaginatedReviewResult> GetReviewsByTourIdAsync(Guid tourId, int pageIndex, int pageSize);
        Task<List<Guid>> GetUnreviewedBookingIdsAsync(Guid userId, Guid tourId);
        Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto dto);
        Task<ReviewDto> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);
    }
}
