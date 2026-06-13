using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Common.Responses;
using System.IdentityModel.Tokens.Jwt;
using DDMS.Backend.Common.Exceptions;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/promotions")]
public class AdminPromotionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminPromotionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPromotions()
    {
        try
        {
            var list = await _context.promotions
                .Include(p => p.created_byNavigation)
                    .ThenInclude(u => u.user_roles)
                        .ThenInclude(ur => ur.role)
                .OrderByDescending(p => p.created_at)
                .Select(p => new
                {
                    id = p.id,
                    code = p.code,
                    description = p.description,
                    discountType = p.discount_type == "percent" ? "percentage" : p.discount_type,
                    discountValue = p.discount_value,
                    minOrderValue = p.min_order_value,
                    maxDiscount = p.max_discount,
                    usageLimit = p.usage_limit,
                    usedCount = p.used_count,
                    validFrom = p.valid_from,
                    validUntil = p.valid_until,
                    isActive = p.is_active ?? true,
                    status = p.status,
                    createdBy = p.created_by,
                    creatorName = p.created_byNavigation != null ? p.created_byNavigation.full_name : "Hệ thống",
                    creatorEmail = p.created_byNavigation != null ? p.created_byNavigation.email : "",
                    creatorRole = p.created_byNavigation != null && p.created_byNavigation.user_roles.Any(ur => ur.role.name == "admin") ? "admin" : "owner",
                    createdAt = p.created_at
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(list));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { message = "Mã giảm giá không được để trống." });

            if (await _context.promotions.AnyAsync(p => p.code == req.Code.Trim()))
                return BadRequest(new { message = "Mã giảm giá này đã tồn tại trên hệ thống." });

            var adminId = GetCurrentUserId();

            var promo = new promotion
            {
                id = Guid.NewGuid(),
                code = req.Code.Trim().ToUpper(),
                description = req.Description,
                discount_type = req.DiscountType == "percentage" ? "percent" : req.DiscountType,
                discount_value = req.DiscountValue,
                min_order_value = req.MinOrderValue,
                max_discount = req.MaxDiscount,
                usage_limit = req.UsageLimit,
                used_count = 0,
                valid_from = req.ValidFrom,
                valid_until = req.ValidUntil,
                is_active = true,
                status = "approved", // Admin promotions are auto-approved
                created_by = adminId,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _context.promotions.Add(promo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { success = true, id = promo.id }));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return StatusCode(500, new { message = msg });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePromotion(Guid id, [FromBody] CreatePromotionRequest req)
    {
        try
        {
            var promo = await _context.promotions.FindAsync(id);
            if (promo == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            if (promo.code != req.Code.Trim() && await _context.promotions.AnyAsync(p => p.code == req.Code.Trim() && p.id != id))
                return BadRequest(new { message = "Mã giảm giá này đã tồn tại trên hệ thống." });

            promo.code = req.Code.Trim().ToUpper();
            promo.description = req.Description;
            promo.discount_type = req.DiscountType == "percentage" ? "percent" : req.DiscountType;
            promo.discount_value = req.DiscountValue;
            promo.min_order_value = req.MinOrderValue;
            promo.max_discount = req.MaxDiscount;
            promo.usage_limit = req.UsageLimit;
            promo.valid_from = req.ValidFrom;
            promo.valid_until = req.ValidUntil;
            promo.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return StatusCode(500, new { message = msg });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePromotion(Guid id)
    {
        try
        {
            var promo = await _context.promotions.FindAsync(id);
            if (promo == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            _context.promotions.Remove(promo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApprovePromotion(Guid id)
    {
        try
        {
            var promo = await _context.promotions.FindAsync(id);
            if (promo == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            promo.status = "approved";
            promo.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectPromotion(Guid id)
    {
        try
        {
            var promo = await _context.promotions.FindAsync(id);
            if (promo == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            promo.status = "rejected";
            promo.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        try
        {
            var promo = await _context.promotions.FindAsync(id);
            if (promo == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            promo.is_active = !(promo.is_active ?? true);
            promo.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { success = true, isActive = promo.is_active }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();
        return userId;
    }
}

public class CreatePromotionRequest
{
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!; // percentage / fixed
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}
