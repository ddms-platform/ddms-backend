using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly ISystemService _system;
    public SystemController(ISystemService system) => _system = system;

    [HttpGet("boat-types")]
    public async Task<IActionResult> GetBoatTypes(CancellationToken ct) =>
        Ok(await _system.GetBoatTypesAsync(ct));
}
