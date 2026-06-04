namespace DDMS.Backend.Models.DTOs.Admin.Users;

public class AdminUserListQuery
{
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
    public string? search { get; set; }
    public string? role { get; set; }
    public bool? isActive { get; set; }
    public bool? emailVerified { get; set; }
}
