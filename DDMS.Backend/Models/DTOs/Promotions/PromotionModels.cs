namespace DDMS.Backend.Models.DTOs.Promotions;

public class PromotionItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminPromotionItem : PromotionItem
{
    public Guid? CreatedBy { get; set; }
    // null khi không xác định được người tạo (vd: hệ thống tự sinh, user bị xoá).
    // FE chịu trách nhiệm hiển thị fallback theo locale ("Hệ thống" / "System" / ...).
    public string? CreatorName { get; set; }
    public string? CreatorEmail { get; set; }
    public string? CreatorRole { get; set; }
}

public class CreatePromotionRequest
{
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}
