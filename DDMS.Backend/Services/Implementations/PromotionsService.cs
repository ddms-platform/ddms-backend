using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Promotions;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class OwnerPromotionsService : IOwnerPromotionsService
{
    private readonly IPromotionsRepository _repo;
    public OwnerPromotionsService(IPromotionsRepository repo) => _repo = repo;

    public async Task<List<PromotionItem>> GetMineAsync(Guid ownerId, CancellationToken ct)
    {
        var list = await _repo.GetByOwnerAsync(ownerId, ct);
        return list.Select(PromotionMapper.MapItem).ToList();
    }

    public async Task<Guid> CreateAsync(Guid ownerId, CreatePromotionRequest req, CancellationToken ct)
    {
        var code = PromotionMapper.NormalizeCode(req.Code);
        if (await _repo.CodeExistsAsync(code, null, ct))
            throw new AppException(ErrorCode.PromotionCodeExists, ErrorCode.Messages.PromotionCodeExists);

        var promo = PromotionMapper.BuildEntity(req, ownerId, PromotionStatuses.Pending);
        _repo.Add(promo);
        await _repo.SaveChangesAsync(ct);
        return promo.id;
    }

    public async Task DeleteAsync(Guid promotionId, Guid ownerId, CancellationToken ct)
    {
        var promo = await _repo.FindAsync(promotionId, ct)
            ?? throw new NotFoundException(ErrorCode.PromotionNotFound, ErrorCode.Messages.PromotionNotFound);

        if (promo.created_by != ownerId)
            throw new ForbiddenException();

        _repo.Remove(promo);
        await _repo.SaveChangesAsync(ct);
    }
}

public class AdminPromotionsService : IAdminPromotionsService
{
    private readonly IPromotionsRepository _repo;
    public AdminPromotionsService(IPromotionsRepository repo) => _repo = repo;

    public async Task<List<AdminPromotionItem>> GetAllAsync(CancellationToken ct)
    {
        var list = await _repo.GetAllWithCreatorAsync(ct);
        return list.Select(PromotionMapper.MapAdminItem).ToList();
    }

    public async Task<Guid> CreateAsync(Guid adminId, CreatePromotionRequest req, CancellationToken ct)
    {
        var code = PromotionMapper.NormalizeCode(req.Code);
        if (await _repo.CodeExistsAsync(code, null, ct))
            throw new AppException(ErrorCode.PromotionCodeExists, ErrorCode.Messages.PromotionCodeExists);

        var promo = PromotionMapper.BuildEntity(req, adminId, PromotionStatuses.Approved);
        _repo.Add(promo);
        await _repo.SaveChangesAsync(ct);
        return promo.id;
    }

    public async Task UpdateAsync(Guid id, CreatePromotionRequest req, CancellationToken ct)
    {
        var promo = await _repo.FindAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.PromotionNotFound, ErrorCode.Messages.PromotionNotFound);

        var code = PromotionMapper.NormalizeCode(req.Code);
        if (promo.code != code && await _repo.CodeExistsAsync(code, id, ct))
            throw new AppException(ErrorCode.PromotionCodeExists, ErrorCode.Messages.PromotionCodeExists);

        PromotionMapper.ApplyUpdate(promo, req);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var promo = await _repo.FindAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.PromotionNotFound, ErrorCode.Messages.PromotionNotFound);
        _repo.Remove(promo);
        await _repo.SaveChangesAsync(ct);
    }

    public Task ApproveAsync(Guid id, CancellationToken ct) =>
        SetStatusAsync(id, PromotionStatuses.Approved, ct);

    public Task RejectAsync(Guid id, CancellationToken ct) =>
        SetStatusAsync(id, PromotionStatuses.Rejected, ct);

    public async Task<bool> ToggleActiveAsync(Guid id, CancellationToken ct)
    {
        var promo = await _repo.FindAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.PromotionNotFound, ErrorCode.Messages.PromotionNotFound);
        promo.is_active = !(promo.is_active ?? true);
        promo.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);
        return promo.is_active.Value;
    }

    private async Task SetStatusAsync(Guid id, string status, CancellationToken ct)
    {
        var promo = await _repo.FindAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.PromotionNotFound, ErrorCode.Messages.PromotionNotFound);
        promo.status = status;
        promo.updated_at = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);
    }
}

internal static class PromotionMapper
{
    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new AppException(ErrorCode.PromotionCodeRequired, ErrorCode.Messages.PromotionCodeRequired);
        return code.Trim().ToUpper();
    }

    public static promotion BuildEntity(CreatePromotionRequest req, Guid createdBy, string status) => new()
    {
        id = Guid.NewGuid(),
        code = NormalizeCode(req.Code),
        description = req.Description,
        discount_type = DiscountTypes.ToDb(req.DiscountType),
        discount_value = req.DiscountValue,
        min_order_value = req.MinOrderValue,
        max_discount = req.MaxDiscount,
        usage_limit = req.UsageLimit,
        used_count = 0,
        valid_from = req.ValidFrom,
        valid_until = req.ValidUntil,
        is_active = true,
        status = status,
        created_by = createdBy,
        created_at = DateTime.UtcNow,
        updated_at = DateTime.UtcNow
    };

    public static void ApplyUpdate(promotion promo, CreatePromotionRequest req)
    {
        promo.code = NormalizeCode(req.Code);
        promo.description = req.Description;
        promo.discount_type = DiscountTypes.ToDb(req.DiscountType);
        promo.discount_value = req.DiscountValue;
        promo.min_order_value = req.MinOrderValue;
        promo.max_discount = req.MaxDiscount;
        promo.usage_limit = req.UsageLimit;
        promo.valid_from = req.ValidFrom;
        promo.valid_until = req.ValidUntil;
        promo.updated_at = DateTime.UtcNow;
    }

    public static PromotionItem MapItem(promotion p) => new()
    {
        Id = p.id,
        Code = p.code,
        Description = p.description,
        DiscountType = DiscountTypes.ToApi(p.discount_type),
        DiscountValue = p.discount_value,
        MinOrderValue = p.min_order_value,
        MaxDiscount = p.max_discount,
        UsageLimit = p.usage_limit,
        UsedCount = p.used_count,
        ValidFrom = p.valid_from,
        ValidUntil = p.valid_until,
        IsActive = p.is_active ?? true,
        Status = p.status,
        CreatedAt = p.created_at
    };

    public static AdminPromotionItem MapAdminItem(promotion p)
    {
        var item = new AdminPromotionItem
        {
            Id = p.id, Code = p.code, Description = p.description,
            DiscountType = DiscountTypes.ToApi(p.discount_type),
            DiscountValue = p.discount_value, MinOrderValue = p.min_order_value,
            MaxDiscount = p.max_discount, UsageLimit = p.usage_limit,
            UsedCount = p.used_count, ValidFrom = p.valid_from,
            ValidUntil = p.valid_until, IsActive = p.is_active ?? true,
            Status = p.status, CreatedAt = p.created_at,
            CreatedBy = p.created_by,
            CreatorName = p.created_byNavigation?.full_name ?? "Hệ thống",
            CreatorEmail = p.created_byNavigation?.email ?? ""
        };
        if (p.created_byNavigation != null
            && p.created_byNavigation.user_roles.Any(ur => ur.role.name == "admin"))
            item.CreatorRole = "admin";
        return item;
    }
}
