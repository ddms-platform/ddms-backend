using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IWishlistRepository
{
    Task<List<wishlist>> GetWishlistsByUserIdAsync(Guid userId);
    Task<wishlist?> GetWishlistAsync(Guid userId, Guid tourId);
    Task AddWishlistAsync(wishlist wishlist);
    Task RemoveWishlistAsync(wishlist wishlist);
    Task<List<Guid>> GetWishlistedTourIdsAsync(Guid userId);
}
