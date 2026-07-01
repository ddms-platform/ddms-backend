using System;
using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Models.DTOs.Wishlists;

public class WishlistToggleRequest
{
    [Required]
    public Guid TourId { get; set; }
}
