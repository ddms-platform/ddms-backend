using DDMS.Backend.Data;
using DDMS.Backend.DTOs.Boats;
using DDMS.Backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/boats")]
public class BoatsController(AppDbContext db) : ControllerBase
{
    // ── GET /api/boats ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = db.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.boat_images)
            .Include(b => b.boat_maintenances)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.status == status);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(b => b.type == type);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.name.Contains(search));

        var total = await query.CountAsync();
        var boats = await query
            .OrderByDescending(b => b.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data = boats.Select(ToDto),
            total,
            page,
            pageSize
        });
    }

    // ── GET /api/boats/{id} ───────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var boat = await db.boats
            .Include(b => b.boat_cabins)
            .Include(b => b.boat_services)
            .Include(b => b.boat_images)
            .Include(b => b.boat_maintenances)
            .FirstOrDefaultAsync(b => b.id == id);

        if (boat is null)
            return NotFound(new { message = "Không tìm thấy tàu" });

        return Ok(ToDto(boat));
    }

    // ── POST /api/boats ───────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoatDto dto)
    {
        var boat = new boat
        {
            id = Guid.NewGuid(),
            name = dto.Name,
            type = dto.Type,
            max_passengers = dto.MaxPassengers,
            status = string.IsNullOrWhiteSpace(dto.Status) ? "idle" : dto.Status,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        db.boats.Add(boat);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = boat.id }, ToDto(boat));
    }

    // ── PUT /api/boats/{id} ───────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateBoatDto dto)
    {
        var boat = await db.boats.FindAsync(id);
        if (boat is null)
            return NotFound(new { message = "Không tìm thấy tàu" });

        boat.name = dto.Name;
        boat.type = dto.Type;
        boat.max_passengers = dto.MaxPassengers;
        if (!string.IsNullOrWhiteSpace(dto.Status))
            boat.status = dto.Status;

        await db.SaveChangesAsync();

        // Reload with relations
        await db.Entry(boat).Collection(b => b.boat_cabins).LoadAsync();
        await db.Entry(boat).Collection(b => b.boat_services).LoadAsync();
        await db.Entry(boat).Collection(b => b.boat_images).LoadAsync();
        await db.Entry(boat).Collection(b => b.boat_maintenances).LoadAsync();

        return Ok(ToDto(boat));
    }

    // ── PATCH /api/boats/{id}/status ─────────────────────────────────────────
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBoatStatusDto dto)
    {
        var boat = await db.boats.FindAsync(id);
        if (boat is null)
            return NotFound(new { message = "Không tìm thấy tàu" });

        boat.status = dto.Status;
        await db.SaveChangesAsync();

        return Ok(new { id = boat.id, status = boat.status });
    }

    // ── DELETE /api/boats/{id} ────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var boat = await db.boats.FindAsync(id);
        if (boat is null)
            return NotFound(new { message = "Không tìm thấy tàu" });

        db.boats.Remove(boat);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ── POST /api/boats/{id}/maintenances ─────────────────────────────────────
    [HttpPost("{id:guid}/maintenances")]
    public async Task<IActionResult> AddMaintenance(Guid id, [FromBody] CreateBoatMaintenanceDto dto)
    {
        var boat = await db.boats.FindAsync(id);
        if (boat is null)
            return NotFound(new { message = "Không tìm thấy tàu" });

        var maintenance = new boat_maintenance
        {
            id = Guid.NewGuid(),
            boat_id = id,
            start_time = dto.StartTime,
            end_time = dto.EndTime,
            reason = dto.Reason,
            created_at = DateTime.UtcNow
        };

        db.boat_maintenances.Add(maintenance);
        await db.SaveChangesAsync();

        return Ok(new BoatMaintenanceDto
        {
            Id = maintenance.id,
            BoatId = maintenance.boat_id,
            StartTime = maintenance.start_time,
            EndTime = maintenance.end_time,
            Reason = maintenance.reason,
            CreatedAt = maintenance.created_at
        });
    }

    // ── DELETE /api/boats/{id}/maintenances/{maintenanceId} ──────────────────
    [HttpDelete("{id:guid}/maintenances/{maintenanceId:guid}")]
    public async Task<IActionResult> DeleteMaintenance(Guid id, Guid maintenanceId)
    {
        var maintenance = await db.boat_maintenances
            .FirstOrDefaultAsync(m => m.id == maintenanceId && m.boat_id == id);

        if (maintenance is null)
            return NotFound(new { message = "Không tìm thấy lịch bảo trì" });

        db.boat_maintenances.Remove(maintenance);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static BoatDto ToDto(boat b) => new()
    {
        Id = b.id,
        Name = b.name,
        Type = b.type,
        MaxPassengers = b.max_passengers,
        Status = b.status,
        CreatedAt = b.created_at,
        UpdatedAt = b.updated_at,
        TotalCabins = b.boat_cabins.Sum(c => c.total_rooms),
        TotalServices = b.boat_services.Count,
        ActiveServices = b.boat_services.Count(s => s.is_active == true),
        Cabins = b.boat_cabins.Select(c => new BoatCabinDto
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
        }).ToList(),
        Services = b.boat_services.Select(s => new BoatServiceDto
        {
            Id = s.id,
            BoatId = s.boat_id,
            Name = s.name,
            Price = s.price,
            Description = s.description,
            IsActive = s.is_active ?? true,
            CreatedAt = s.created_at,
            UpdatedAt = s.updated_at
        }).ToList(),
        Images = b.boat_images
            .OrderBy(i => i.sort_order)
            .Select(i => new BoatImageDto
            {
                Id = i.id,
                BoatId = i.boat_id,
                ImageUrl = i.image_url,
                PublicId = i.public_id,
                Caption = i.caption,
                SortOrder = i.sort_order
            }).ToList(),
        Maintenances = b.boat_maintenances.Select(m => new BoatMaintenanceDto
        {
            Id = m.id,
            BoatId = m.boat_id,
            StartTime = m.start_time,
            EndTime = m.end_time,
            Reason = m.reason,
            CreatedAt = m.created_at
        }).ToList()
    };
}

public record UpdateBoatStatusDto(string Status);
