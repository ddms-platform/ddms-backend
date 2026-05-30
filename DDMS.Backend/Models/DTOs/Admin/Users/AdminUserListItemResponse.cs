namespace DDMS.Backend.Models.DTOs.Admin.Users;

public class AdminUserListItemResponse
{
    public Guid id { get; set; }
    public string fullName { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string? phone { get; set; }
    public List<string> roles { get; set; } = [];
    public bool isActive { get; set; }
    public bool emailVerified { get; set; }
    public bool ownerVerified { get; set; }
    public DateTime createdAt { get; set; }
}
