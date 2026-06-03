using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IFaqService
{
    Task<List<FaqItemResponse>> GetByTourIdAsync(Guid tourId, Guid userId);
    Task<FaqItemResponse> GetByIdAsync(Guid tourId, Guid faqId, Guid userId);
    Task<FaqItemResponse> CreateAsync(Guid tourId, Guid userId, CreateFaqRequest request);
    Task<FaqItemResponse> UpdateAsync(Guid tourId, Guid faqId, Guid userId, UpdateFaqRequest request);
    Task DeleteAsync(Guid tourId, Guid faqId, Guid userId);
}
