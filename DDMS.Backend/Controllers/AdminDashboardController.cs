using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.AdminDashboard;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboard;
    public AdminDashboardController(IAdminDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken ct) =>
        Ok(ApiResponse<AdminDashboardResponse>.Ok(await _dashboard.GetStatsAsync(ct)));

    [HttpGet("top-tours")]
    public async Task<IActionResult> GetTopTours(CancellationToken ct) =>
        Ok(ApiResponse<List<TopTourItem>>.Ok(await _dashboard.GetTopToursAsync(ct)));
}
