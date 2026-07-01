using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.DTOs.Wishlists;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ITourRepository _tourRepository;

    public WishlistService(IWishlistRepository wishlistRepository, ITourRepository tourRepository)
    {
        _wishlistRepository = wishlistRepository;
        _tourRepository = tourRepository;
    }

    public async Task<List<TourSearchItemResponse>> GetWishlistToursAsync(Guid userId)
    {
        var wishlists = await _wishlistRepository.GetWishlistsByUserIdAsync(userId);
        
        var result = new List<TourSearchItemResponse>();
        foreach (var w in wishlists)
        {
            var tour = w.tour;
            if (tour == null) continue;

            result.Add(new TourSearchItemResponse
            {
                id = tour.id,
                name = tour.name,
                price = tour.price,
                description = tour.description,
                durationMinutes = tour.duration_minutes,
                location = tour.location,
                status = tour.status,
                avgRating = tour.avg_rating,
                totalReviews = tour.total_reviews,
                cancelPolicy = tour.cancel_policy,
                cancelHours = tour.cancel_hours,
                imageUrl = tour.tour_images?.OrderBy(i => i.sort_order).FirstOrDefault()?.image_url,
                availableSlots = [] // Simplification, normally fetched if needed
            });
        }
        
        return result;
    }

    public async Task<bool> ToggleWishlistAsync(Guid userId, WishlistToggleRequest request)
    {
        var existing = await _wishlistRepository.GetWishlistAsync(userId, request.TourId);
        if (existing != null)
        {
            await _wishlistRepository.RemoveWishlistAsync(existing);
            return false; // Removed
        }
        
        var wishlist = new wishlist
        {
            id = Guid.NewGuid(),
            user_id = userId,
            tour_id = request.TourId
        };
        await _wishlistRepository.AddWishlistAsync(wishlist);
        return true; // Added
    }
    
    public async Task<List<Guid>> GetWishlistedTourIdsAsync(Guid userId)
    {
        return await _wishlistRepository.GetWishlistedTourIdsAsync(userId);
    }
}
