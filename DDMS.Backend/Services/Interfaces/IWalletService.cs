using DDMS.Backend.Models.DTOs.Wallet;

namespace DDMS.Backend.Services.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceResponse> GetBalanceAsync(Guid userId, CancellationToken ct);
    Task<List<WithdrawalListItem>> GetWithdrawalsAsync(Guid userId, CancellationToken ct);
    Task<WithdrawResult> RequestWithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken ct);
}
