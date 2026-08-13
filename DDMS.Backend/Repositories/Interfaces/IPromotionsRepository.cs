using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IPromotionsRepository
{
    Task<List<promotion>> GetAllWithCreatorAsync(CancellationToken ct);
    Task<List<promotion>> GetByOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<promotion?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);

    /// <summary>Tra mã giảm giá theo code (không phân biệt hoa thường), kèm vai trò người tạo.</summary>
    Task<promotion?> FindByCodeWithCreatorRolesAsync(string code, CancellationToken ct);

    /// <summary>
    /// Tăng used_count một cách nguyên tử, chỉ khi còn lượt. Trả về false nếu mã vừa hết lượt
    /// do request khác dùng trước — tránh đua khi hai người đặt cùng lúc.
    /// </summary>
    Task<bool> TryConsumeUsageAsync(Guid promotionId, CancellationToken ct);

    void Add(promotion entity);
    void Remove(promotion entity);
    Task SaveChangesAsync(CancellationToken ct);
}
