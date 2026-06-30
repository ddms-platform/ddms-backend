using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.DTOs.Review
{
    public class CreateReviewDto
    {
        public Guid BookingId { get; set; }
        public Guid TourId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<IFormFile> Images { get; set; }
        public List<IFormFile> Videos { get; set; }
    }
}
