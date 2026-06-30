using DDMS.Backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDMS.Backend.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<(List<review> reviews, int totalCount)> GetReviewsByTourIdAsync(Guid tourId, int pageIndex, int pageSize);
        Task<List<booking>> GetUnreviewedCompletedBookingsAsync(Guid userId, Guid tourId);
        Task<review> GetReviewByIdAsync(Guid id);
        Task<review> AddReviewAsync(review review);
        Task<review> UpdateReviewAsync(review review);
        Task<bool> DeleteReviewAsync(Guid id);
        Task<bool> HasUserReviewedBookingAsync(Guid bookingId);
    }
}
