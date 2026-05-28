namespace DDMS.Backend.Models.DTOs.Admin.Users;

public class AdminUserStatsResponse
{
    public int total { get; set; }
    public int adminCount { get; set; }
    public int ownerCount { get; set; }
    public int userCount { get; set; }
}
