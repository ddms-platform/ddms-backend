using DDMS.Backend.Models.DTOs.Promotions;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerPromotionsService
{
    Task<List<PromotionItem>> GetMineAsync(Guid ownerId, CancellationToken ct);
    Task<Guid> CreateAsync(Guid ownerId, CreatePromotionRequest request, CancellationToken ct);
    Task DeleteAsync(Guid promotionId, Guid ownerId, CancellationToken ct);
}

public interface IAdminPromotionsService
{
    Task<List<AdminPromotionItem>> GetAllAsync(CancellationToken ct);
    Task<Guid> CreateAsync(Guid adminId, CreatePromotionRequest request, CancellationToken ct);
    Task UpdateAsync(Guid id, CreatePromotionRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task ApproveAsync(Guid id, CancellationToken ct);
    Task RejectAsync(Guid id, CancellationToken ct);
    Task<bool> ToggleActiveAsync(Guid id, CancellationToken ct);
}
