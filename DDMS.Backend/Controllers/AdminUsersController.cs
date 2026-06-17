using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Admin.Users;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDMS.Backend.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly ICurrentUser _user;

    public AdminUsersController(IAdminUserService adminUserService, ICurrentUser user)
    {
        _adminUserService = adminUserService;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] AdminUserListQuery query)
    {
        var result = await _adminUserService.GetUsersAsync(query);
        return Ok(ApiResponse<PagedResponse<AdminUserListItemResponse>>.Ok(result));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _adminUserService.GetStatsAsync();
        return Ok(ApiResponse<AdminUserStatsResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _adminUserService.GetByIdAsync(id);
        return Ok(ApiResponse<AdminUserListItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdminUserRequest request)
    {
        var result = await _adminUserService.UpdateAsync(id, request, _user.Id);
        return Ok(ApiResponse<AdminUserListItemResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid id, [FromBody] UpdateAdminUserRolesRequest request)
    {
        var result = await _adminUserService.UpdateRolesAsync(id, request, _user.Id);
        return Ok(ApiResponse<AdminUserListItemResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _adminUserService.DeactivateAsync(id, _user.Id);
        return Ok(ApiResponse<object>.Ok(new { deactivated = true }));
    }
}
