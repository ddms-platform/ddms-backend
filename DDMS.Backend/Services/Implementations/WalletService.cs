using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Wallet;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _wallets;
    private readonly IWithdrawalsRepository _withdrawals;

    public WalletService(IWalletRepository wallets, IWithdrawalsRepository withdrawals)
    {
        _wallets = wallets;
        _withdrawals = withdrawals;
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(Guid userId, CancellationToken ct)
    {
        var wallet = await _wallets.FindAsync(userId, ct);
        return new WalletBalanceResponse { Balance = wallet?.balance ?? 0m };
    }

    public Task<List<WithdrawalListItem>> GetWithdrawalsAsync(Guid userId, CancellationToken ct) =>
        _withdrawals.GetByUserAsync(userId, ct);

    public async Task<WithdrawResult> RequestWithdrawAsync(Guid userId, WithdrawRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            throw new AppException(ErrorCode.WithdrawAmountInvalid, ErrorCode.Messages.WithdrawAmountInvalid);

        if (string.IsNullOrWhiteSpace(req.BankName)
         || string.IsNullOrWhiteSpace(req.AccountNumber)
         || string.IsNullOrWhiteSpace(req.AccountName))
            throw new AppException(ErrorCode.WithdrawBankInfoRequired, ErrorCode.Messages.WithdrawBankInfoRequired);

        var wallet = await _wallets.FindAsync(userId, ct);
        if (wallet == null || wallet.balance < req.Amount)
            throw new AppException(ErrorCode.WithdrawInsufficientBalance, ErrorCode.Messages.WithdrawInsufficientBalance);

        var now = DateTime.UtcNow;
        wallet.balance -= req.Amount;
        wallet.updated_at = now;

        _withdrawals.Add(new wallet_withdrawal
        {
            id = Guid.NewGuid(),
            user_id = userId,
            amount = req.Amount,
            bank_name = req.BankName.Trim(),
            account_number = req.AccountNumber.Trim(),
            account_name = req.AccountName.Trim(),
            status = WithdrawalStatuses.Pending,
            created_at = now
        });

        await _withdrawals.SaveChangesAsync(ct);
        return new WithdrawResult { NewBalance = wallet.balance };
    }
}
