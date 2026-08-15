namespace DDMS.Backend.Models.DTOs.Auth;

public class CurrentUserResponse
{
    public Guid id { get; set; }
    public string fullName { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public List<string> roles { get; set; } = [];
    public bool emailVerified { get; set; }
    public string? phone { get; set; }
    public string? address { get; set; }
    public string? avatarUrl { get; set; }
    public bool hasOwnerProfile { get; set; }
    public string? ownerProfileStatus { get; set; }
}
