namespace DDMS.Backend.Models.DTOs.Admin.Users;

public class UpdateAdminUserRequest
{
    public string fullName { get; set; } = string.Empty;
    public string? phone { get; set; }
    public bool isActive { get; set; } = true;
}
