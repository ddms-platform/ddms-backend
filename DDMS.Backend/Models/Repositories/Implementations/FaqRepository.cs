using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Models.Repositories.Implementations;

public class FaqRepository : IFaqRepository
{
    private readonly AppDbContext _dbContext;

    public FaqRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<faq>> GetByTourIdAsync(Guid tourId)
    {
        return _dbContext.faqs
            .Where(x => x.tour_id == tourId)
            .OrderBy(x => x.sort_order)
            .ThenBy(x => x.created_at)
            .ToListAsync();
    }

    public Task<faq?> GetByIdAsync(Guid id, Guid tourId)
    {
        return _dbContext.faqs
            .FirstOrDefaultAsync(x => x.id == id && x.tour_id == tourId);
    }

    public async Task AddAsync(faq entity)
    {
        _dbContext.faqs.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(faq entity)
    {
        _dbContext.faqs.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(faq entity)
    {
        _dbContext.faqs.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}
