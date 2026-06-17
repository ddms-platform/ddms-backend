using DDMS.Backend.Models.DTOs.MaintenanceServices;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/owner/maintenance-services")]
public class MaintenanceServicesController : ControllerBase
{
    private readonly IMaintenanceServicesService _svc;
    public MaintenanceServicesController(IMaintenanceServicesService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetMaintenanceServices(CancellationToken ct)
    {
        var result = await _svc.GetAllAsync(ct);
        return Ok(new { IsSuccess = true, Result = result });
    }
}
