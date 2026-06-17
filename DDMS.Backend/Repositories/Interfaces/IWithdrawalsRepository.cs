using DDMS.Backend.Models.DTOs.Wallet;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IWithdrawalsRepository
{
    Task<List<WithdrawalListItem>> GetByUserAsync(Guid userId, CancellationToken ct);
    void Add(wallet_withdrawal entity);
    Task SaveChangesAsync(CancellationToken ct);
}
