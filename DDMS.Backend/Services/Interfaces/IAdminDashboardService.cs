using DDMS.Backend.Models.DTOs.AdminDashboard;

namespace DDMS.Backend.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardResponse> GetStatsAsync(CancellationToken ct);
    Task<List<TopTourItem>> GetTopToursAsync(CancellationToken ct);
}
