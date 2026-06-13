using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/maintenances")]
public class AdminMaintenancesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;

    public AdminMaintenancesController(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingMaintenances()
    {
        try
        {
            var pending = await _context.boat_maintenances
                .Include(m => m.boat)
                .Include(m => m.port_maintenance_service)
                .Where(m => m.status == "pending")
                .OrderByDescending(m => m.created_at)
                .Select(m => new
                {
                    id = m.id,
                    boatId = m.boat_id,
                    boatName = m.boat.name,
                    startTime = m.start_time,
                    endTime = m.end_time,
                    reason = m.reason,
                    createdAt = m.created_at,
                    portMaintenanceServiceId = m.port_maintenance_service_id,
                    portMaintenanceServiceName = m.port_maintenance_service != null ? m.port_maintenance_service.name : m.reason,
                    status = m.status
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(pending));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "Lỗi khi lấy danh sách bảo trì chờ duyệt",
                Error = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMaintenances()
    {
        try
        {
            var list = await _context.boat_maintenances
                .Include(m => m.boat)
                .Include(m => m.port_maintenance_service)
                .OrderByDescending(m => m.created_at)
                .Select(m => new
                {
                    id = m.id,
                    boatId = m.boat_id,
                    boatName = m.boat.name,
                    startTime = m.start_time,
                    endTime = m.end_time,
                    reason = m.reason,
                    createdAt = m.created_at,
                    portMaintenanceServiceId = m.port_maintenance_service_id,
                    portMaintenanceServiceName = m.port_maintenance_service != null ? m.port_maintenance_service.name : m.reason,
                    price = m.port_maintenance_service != null ? (m.port_maintenance_service.price ?? 0) : 0,
                    status = m.status
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(list));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "Lỗi khi lấy danh sách tất cả bảo trì",
                Error = ex.Message
            });
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveMaintenance(Guid id)
    {
        try
        {
            var m = await _context.boat_maintenances
                .Include(x => x.boat)
                    .ThenInclude(b => b.owner)
                .Include(x => x.port_maintenance_service)
                .FirstOrDefaultAsync(x => x.id == id);

            if (m == null) return NotFound(new { message = "Không tìm thấy yêu cầu bảo trì." });

            m.status = "approved";
            await _context.SaveChangesAsync();

            // Send notification email to owner
            if (m.boat?.owner != null)
            {
                var price = m.port_maintenance_service?.price ?? 0;
                var serviceName = m.port_maintenance_service != null ? m.port_maintenance_service.name : m.reason ?? "Dịch vụ bảo trì";
                try
                {
                    await _emailSender.SendMaintenanceStatusEmailAsync(
                        m.boat.owner.email,
                        m.boat.owner.full_name,
                        m.boat.name,
                        serviceName,
                        "approved",
                        price
                    );
                }
                catch (Exception mailEx)
                {
                    // Log mail exception but still return success for database state change
                }
            }

            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectMaintenance(Guid id)
    {
        try
        {
            var m = await _context.boat_maintenances
                .Include(x => x.boat)
                    .ThenInclude(b => b.owner)
                .Include(x => x.port_maintenance_service)
                .FirstOrDefaultAsync(x => x.id == id);

            if (m == null) return NotFound(new { message = "Không tìm thấy yêu cầu bảo trì." });

            m.status = "rejected";
            await _context.SaveChangesAsync();

            // Send notification email to owner
            if (m.boat?.owner != null)
            {
                var price = m.port_maintenance_service?.price ?? 0;
                var serviceName = m.port_maintenance_service != null ? m.port_maintenance_service.name : m.reason ?? "Dịch vụ bảo trì";
                try
                {
                    await _emailSender.SendMaintenanceStatusEmailAsync(
                        m.boat.owner.email,
                        m.boat.owner.full_name,
                        m.boat.name,
                        serviceName,
                        "rejected",
                        price
                    );
                }
                catch (Exception mailEx)
                {
                    // Log email sending error
                }
            }

            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
