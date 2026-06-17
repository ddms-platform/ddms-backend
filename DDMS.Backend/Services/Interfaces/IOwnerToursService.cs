using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Tours;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerToursService
{
    Task<PagedResponse<TourItemResponse>> GetToursAsync(Guid userId, TourListQuery query);
    Task<TourItemResponse> GetByIdAsync(Guid id, Guid userId);
    Task<TourItemResponse> CreateAsync(Guid userId, CreateTourRequest request);
    Task<TourItemResponse> UpdateAsync(Guid id, Guid userId, UpdateTourRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}
