using DDMS.Backend.Models.DTOs.AdminWithdrawals;

namespace DDMS.Backend.Services.Interfaces;

public interface IAdminWithdrawalsService
{
    Task<List<WithdrawalItem>> GetAllAsync(CancellationToken ct);
    Task ApproveAsync(Guid id, CancellationToken ct);
    Task RejectAsync(Guid id, CancellationToken ct);
}
