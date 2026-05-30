namespace DDMS.Backend.Models.DTOs.Auth;

public class CurrentUserResponse
{
    public Guid id { get; set; }
    public string fullName { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public List<string> roles { get; set; } = [];
    public bool emailVerified { get; set; }
}
