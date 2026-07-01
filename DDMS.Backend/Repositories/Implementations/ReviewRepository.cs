using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDMS.Backend.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<review> reviews, int totalCount)> GetReviewsByTourIdAsync(Guid tourId, int pageIndex, int pageSize)
        {
            var query = _context.reviews
                .Include(r => r.user)
                .Where(r => r.tour_id == tourId);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.created_at)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<List<booking>> GetUnreviewedCompletedBookingsAsync(Guid userId, Guid tourId)
        {
            var bookings = await _context.bookings
                .Include(b => b.schedule)
                .Include(b => b.review)
                .Where(b => b.user_id == userId && b.schedule.tour_id == tourId && b.status == "completed" && b.review == null)
                .ToListAsync();

            return bookings;
        }

        public async Task<review> GetReviewByIdAsync(Guid id)
        {
            return await _context.reviews.FirstOrDefaultAsync(r => r.id == id);
        }

        public async Task<review> AddReviewAsync(review review)
        {
            _context.reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<review> UpdateReviewAsync(review review)
        {
            _context.reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteReviewAsync(Guid id)
        {
            var review = await _context.reviews.FirstOrDefaultAsync(r => r.id == id);
            if (review != null)
            {
                _context.reviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> HasUserReviewedBookingAsync(Guid bookingId)
        {
            return await _context.reviews.AnyAsync(r => r.booking_id == bookingId);
        }
    }
}
