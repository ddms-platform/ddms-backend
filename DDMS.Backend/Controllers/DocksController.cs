using DDMS.Backend.Data;
using DDMS.Backend.DTOs.Docks;
using DDMS.Backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/docks")]
public class DocksController(AppDbContext db) : ControllerBase
{
    // ── GET /api/docks ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = db.docks
            .Include(d => d.dock_schedules)
                .ThenInclude(ds => ds.boat)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.name.Contains(search) || (d.location != null && d.location.Contains(search)));

        var total = await query.CountAsync();
        var docks = await query
            .OrderByDescending(d => d.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data = docks.Select(ToDto),
            total,
            page,
            pageSize
        });
    }

    // ── GET /api/docks/{id} ───────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dock = await db.docks
            .Include(d => d.dock_schedules)
                .ThenInclude(ds => ds.boat)
            .FirstOrDefaultAsync(d => d.id == id);

        if (dock is null)
            return NotFound(new { message = "Không tìm thấy bến tàu" });

        return Ok(ToDto(dock));
    }

    // ── POST /api/docks ───────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDockDto dto)
    {
        var dock = new dock
        {
            id = Guid.NewGuid(),
            name = dto.Name,
            location = dto.Location,
            max_boats = dto.MaxBoats,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        db.docks.Add(dock);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = dock.id }, ToDto(dock));
    }

    // ── PUT /api/docks/{id} ───────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateDockDto dto)
    {
        var dock = await db.docks.FindAsync(id);
        if (dock is null)
            return NotFound(new { message = "Không tìm thấy bến tàu" });

        dock.name = dto.Name;
        dock.location = dto.Location;
        dock.max_boats = dto.MaxBoats;

        await db.SaveChangesAsync();

        await db.Entry(dock).Collection(d => d.dock_schedules).LoadAsync();
        return Ok(ToDto(dock));
    }

    // ── DELETE /api/docks/{id} ────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dock = await db.docks.FindAsync(id);
        if (dock is null)
            return NotFound(new { message = "Không tìm thấy bến tàu" });

        db.docks.Remove(dock);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ── GET /api/docks/{id}/schedules ─────────────────────────────────────────
    [HttpGet("{id:guid}/schedules")]
    public async Task<IActionResult> GetSchedules(Guid id)
    {
        var exists = await db.docks.AnyAsync(d => d.id == id);
        if (!exists)
            return NotFound(new { message = "Không tìm thấy bến tàu" });

        var schedules = await db.dock_schedules
            .Include(ds => ds.boat)
            .Where(ds => ds.dock_id == id)
            .OrderBy(ds => ds.start_time)
            .Select(ds => ToScheduleDto(ds))
            .ToListAsync();

        return Ok(schedules);
    }

    // ── POST /api/docks/{id}/schedules ────────────────────────────────────────
    [HttpPost("{id:guid}/schedules")]
    public async Task<IActionResult> AddSchedule(Guid id, [FromBody] CreateDockScheduleDto dto)
    {
        var dock = await db.docks.FindAsync(id);
        if (dock is null)
            return NotFound(new { message = "Không tìm thấy bến tàu" });

        // Validate boat exists
        var boatExists = await db.boats.AnyAsync(b => b.id == dto.BoatId);
        if (!boatExists)
            return BadRequest(new { message = "Không tìm thấy tàu" });

        // Check conflict: same dock, overlapping time
        var conflict = await db.dock_schedules.AnyAsync(ds =>
            ds.dock_id == id &&
            ds.boat_id == dto.BoatId &&
            ds.start_time < dto.EndTime &&
            ds.end_time > dto.StartTime);

        if (conflict)
            return Conflict(new { message = "Tàu đã có lịch neo đậu trong khoảng thời gian này" });

        var schedule = new dock_schedule
        {
            id = Guid.NewGuid(),
            dock_id = id,
            boat_id = dto.BoatId,
            schedule_id = dto.ScheduleId,
            start_time = dto.StartTime,
            end_time = dto.EndTime,
            created_at = DateTime.UtcNow
        };

        db.dock_schedules.Add(schedule);
        await db.SaveChangesAsync();

        // Load boat for response
        await db.Entry(schedule).Reference(ds => ds.boat).LoadAsync();

        return Ok(ToScheduleDto(schedule));
    }

    // ── DELETE /api/docks/{id}/schedules/{scheduleId} ─────────────────────────
    [HttpDelete("{id:guid}/schedules/{scheduleId:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id, Guid scheduleId)
    {
        var schedule = await db.dock_schedules
            .FirstOrDefaultAsync(ds => ds.id == scheduleId && ds.dock_id == id);

        if (schedule is null)
            return NotFound(new { message = "Không tìm thấy lịch neo đậu" });

        db.dock_schedules.Remove(schedule);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ── Mappers ───────────────────────────────────────────────────────────────
    private static DockDto ToDto(dock d) => new()
    {
        Id = d.id,
        Name = d.name,
        Location = d.location,
        MaxBoats = d.max_boats,
        CreatedAt = d.created_at,
        UpdatedAt = d.updated_at,
        CurrentBoats = d.dock_schedules
            .Count(ds => ds.start_time <= DateTime.UtcNow && ds.end_time >= DateTime.UtcNow),
        Schedules = d.dock_schedules
            .OrderBy(ds => ds.start_time)
            .Select(ds => ToScheduleDto(ds))
            .ToList()
    };

    private static DockScheduleDto ToScheduleDto(dock_schedule ds) => new()
    {
        Id = ds.id,
        DockId = ds.dock_id,
        BoatId = ds.boat_id,
        BoatName = ds.boat?.name,
        ScheduleId = ds.schedule_id,
        StartTime = ds.start_time,
        EndTime = ds.end_time,
        CreatedAt = ds.created_at
    };
}
