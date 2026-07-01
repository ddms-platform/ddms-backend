using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.DTOs.Review
{
    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string ExistingImageUrls { get; set; } // JSON string of urls to keep
        public string ExistingVideoUrls { get; set; } // JSON string of urls to keep
        public List<IFormFile> NewImages { get; set; }
        public List<IFormFile> NewVideos { get; set; }
    }
}
