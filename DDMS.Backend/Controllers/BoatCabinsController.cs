using DDMS.Backend.Data;
using DDMS.Backend.DTOs.Boats;
using DDMS.Backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/boats/{boatId:guid}/cabins")]
public class BoatCabinsController(AppDbContext db) : ControllerBase
{
    // ── GET /api/boats/{boatId}/cabins ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid boatId)
    {
        var exists = await db.boats.AnyAsync(b => b.id == boatId);
        if (!exists)
            return NotFound(new { message = "Không tìm thấy tàu" });

        var cabins = await db.boat_cabins
            .Where(c => c.boat_id == boatId)
            .OrderBy(c => c.created_at)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(cabins);
    }

    // ── GET /api/boats/{boatId}/cabins/{id} ───────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid boatId, Guid id)
    {
        var cabin = await db.boat_cabins
            .FirstOrDefaultAsync(c => c.id == id && c.boat_id == boatId);

        if (cabin is null)
            return NotFound(new { message = "Không tìm thấy cabin" });

        return Ok(ToDto(cabin));
    }

    // ── POST /api/boats/{boatId}/cabins ───────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create(Guid boatId, [FromBody] CreateBoatCabinDto dto)
    {
        var exists = await db.boats.AnyAsync(b => b.id == boatId);
        if (!exists)
            return NotFound(new { message = "Không tìm thấy tàu" });

        var cabin = new boat_cabin
        {
            id = Guid.NewGuid(),
            boat_id = boatId,
            name = dto.Name,
            capacity = dto.Capacity,
            price = dto.Price,
            total_rooms = dto.TotalRooms,
            description = dto.Description,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        db.boat_cabins.Add(cabin);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { boatId, id = cabin.id }, ToDto(cabin));
    }

    // ── PUT /api/boats/{boatId}/cabins/{id} ───────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid boatId, Guid id, [FromBody] CreateBoatCabinDto dto)
    {
        var cabin = await db.boat_cabins
            .FirstOrDefaultAsync(c => c.id == id && c.boat_id == boatId);

        if (cabin is null)
            return NotFound(new { message = "Không tìm thấy cabin" });

        cabin.name = dto.Name;
        cabin.capacity = dto.Capacity;
        cabin.price = dto.Price;
        cabin.total_rooms = dto.TotalRooms;
        cabin.description = dto.Description;

        await db.SaveChangesAsync();
        return Ok(ToDto(cabin));
    }

    // ── DELETE /api/boats/{boatId}/cabins/{id} ────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boatId, Guid id)
    {
        var cabin = await db.boat_cabins
            .FirstOrDefaultAsync(c => c.id == id && c.boat_id == boatId);

        if (cabin is null)
            return NotFound(new { message = "Không tìm thấy cabin" });

        db.boat_cabins.Remove(cabin);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static BoatCabinDto ToDto(boat_cabin c) => new()
    {
        Id = c.id,
        BoatId = c.boat_id,
        Name = c.name,
        Capacity = c.capacity,
        Price = c.price,
        TotalRooms = c.total_rooms,
        Description = c.description,
        CreatedAt = c.created_at,
        UpdatedAt = c.updated_at
    };
}
