using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IFaqRepository
{
    Task<List<faq>> GetByTourIdAsync(Guid tourId);
    Task<faq?> GetByIdAsync(Guid id, Guid tourId);
    Task AddAsync(faq entity);
    Task UpdateAsync(faq entity);
    Task DeleteAsync(faq entity);
}
