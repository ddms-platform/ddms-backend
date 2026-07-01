using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Wishlists;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Services.Interfaces;

public interface IWishlistService
{
    Task<List<TourSearchItemResponse>> GetWishlistToursAsync(Guid userId);
    Task<bool> ToggleWishlistAsync(Guid userId, WishlistToggleRequest request);
    Task<List<Guid>> GetWishlistedTourIdsAsync(Guid userId);
}
