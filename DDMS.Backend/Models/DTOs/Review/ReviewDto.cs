using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.DTOs.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatarUrl { get; set; }
        public Guid TourId { get; set; }
        public Guid BookingId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<string> VideoUrls { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PaginatedReviewResult
    {
        public List<ReviewDto> Reviews { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
