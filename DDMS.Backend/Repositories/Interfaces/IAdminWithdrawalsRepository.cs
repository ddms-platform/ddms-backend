using DDMS.Backend.Models.DTOs.AdminWithdrawals;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IAdminWithdrawalsRepository
{
    Task<List<WithdrawalItem>> GetAllAsync(CancellationToken ct);
    Task<wallet_withdrawal?> FindWithUserAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
