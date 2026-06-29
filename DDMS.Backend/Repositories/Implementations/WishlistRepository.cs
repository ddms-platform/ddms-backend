using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;

    public WishlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<wishlist>> GetWishlistsByUserIdAsync(Guid userId)
    {
        return await _context.wishlists
            .Include(w => w.tour)
            .Where(w => w.user_id == userId)
            .OrderByDescending(w => w.created_at)
            .ToListAsync();
    }

    public async Task<wishlist?> GetWishlistAsync(Guid userId, Guid tourId)
    {
        return await _context.wishlists
            .FirstOrDefaultAsync(w => w.user_id == userId && w.tour_id == tourId);
    }

    public async Task AddWishlistAsync(wishlist wishlist)
    {
        await _context.wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveWishlistAsync(wishlist wishlist)
    {
        _context.wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<Guid>> GetWishlistedTourIdsAsync(Guid userId)
    {
        return await _context.wishlists
            .Where(w => w.user_id == userId)
            .Select(w => w.tour_id)
            .ToListAsync();
    }
}
