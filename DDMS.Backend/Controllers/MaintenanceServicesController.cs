using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/owner/maintenance-services")]
public class MaintenanceServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaintenanceServicesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMaintenanceServices()
    {
        try
        {
            var services = await _context.port_maintenance_services
                .OrderBy(s => s.price ?? decimal.MaxValue) // Order by price asc, null at end
                .Select(s => new
                {
                    id = s.id,
                    name = s.name,
                    iconCode = s.icon_code,
                    price = s.price,
                    description = s.description
                })
                .ToListAsync();

            return Ok(new
            {
                IsSuccess = true,
                Result = services
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                IsSuccess = false,
                Message = "Lỗi khi lấy danh sách dịch vụ bảo trì",
                Error = ex.Message
            });
        }
    }
}
