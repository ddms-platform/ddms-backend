using DDMS.Backend.Data;
using DDMS.Backend.DTOs.Boats;
using DDMS.Backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/boats/{boatId:guid}/services")]
public class BoatServicesController(AppDbContext db) : ControllerBase
{
    // ── GET /api/boats/{boatId}/services ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid boatId, [FromQuery] bool? isActive)
    {
        var exists = await db.boats.AnyAsync(b => b.id == boatId);
        if (!exists)
            return NotFound(new { message = "Không tìm thấy tàu" });

        var query = db.boat_services.Where(s => s.boat_id == boatId);

        if (isActive.HasValue)
            query = query.Where(s => s.is_active == isActive.Value);

        var services = await query
            .OrderBy(s => s.created_at)
            .Select(s => ToDto(s))
            .ToListAsync();

        return Ok(services);
    }

    // ── GET /api/boats/{boatId}/services/{id} ─────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid boatId, Guid id)
    {
        var service = await db.boat_services
            .FirstOrDefaultAsync(s => s.id == id && s.boat_id == boatId);

        if (service is null)
            return NotFound(new { message = "Không tìm thấy dịch vụ" });

        return Ok(ToDto(service));
    }

    // ── POST /api/boats/{boatId}/services ─────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create(Guid boatId, [FromBody] CreateBoatServiceDto dto)
    {
        var exists = await db.boats.AnyAsync(b => b.id == boatId);
        if (!exists)
            return NotFound(new { message = "Không tìm thấy tàu" });

        var service = new boat_service
        {
            id = Guid.NewGuid(),
            boat_id = boatId,
            name = dto.Name,
            price = dto.Price,
            description = dto.Description,
            is_active = dto.IsActive,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        db.boat_services.Add(service);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { boatId, id = service.id }, ToDto(service));
    }

    // ── PUT /api/boats/{boatId}/services/{id} ─────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid boatId, Guid id, [FromBody] CreateBoatServiceDto dto)
    {
        var service = await db.boat_services
            .FirstOrDefaultAsync(s => s.id == id && s.boat_id == boatId);

        if (service is null)
            return NotFound(new { message = "Không tìm thấy dịch vụ" });

        service.name = dto.Name;
        service.price = dto.Price;
        service.description = dto.Description;
        service.is_active = dto.IsActive;

        await db.SaveChangesAsync();
        return Ok(ToDto(service));
    }

    // ── PATCH /api/boats/{boatId}/services/{id}/toggle ────────────────────────
    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid boatId, Guid id)
    {
        var service = await db.boat_services
            .FirstOrDefaultAsync(s => s.id == id && s.boat_id == boatId);

        if (service is null)
            return NotFound(new { message = "Không tìm thấy dịch vụ" });

        service.is_active = !(service.is_active ?? true);
        await db.SaveChangesAsync();

        return Ok(new { id = service.id, isActive = service.is_active });
    }

    // ── DELETE /api/boats/{boatId}/services/{id} ──────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boatId, Guid id)
    {
        var service = await db.boat_services
            .FirstOrDefaultAsync(s => s.id == id && s.boat_id == boatId);

        if (service is null)
            return NotFound(new { message = "Không tìm thấy dịch vụ" });

        db.boat_services.Remove(service);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static BoatServiceDto ToDto(boat_service s) => new()
    {
        Id = s.id,
        BoatId = s.boat_id,
        Name = s.name,
        Price = s.price,
        Description = s.description,
        IsActive = s.is_active ?? true,
        CreatedAt = s.created_at,
        UpdatedAt = s.updated_at
    };
}
