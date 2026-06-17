using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.Wallet;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class WithdrawalsRepository : IWithdrawalsRepository
{
    private readonly AppDbContext _db;
    public WithdrawalsRepository(AppDbContext db) => _db = db;

    public Task<List<WithdrawalListItem>> GetByUserAsync(Guid userId, CancellationToken ct) =>
        _db.wallet_withdrawals
            .Where(w => w.user_id == userId)
            .OrderByDescending(w => w.created_at)
            .Select(w => new WithdrawalListItem
            {
                Id = w.id,
                Amount = w.amount,
                BankName = w.bank_name,
                AccountNumber = w.account_number,
                AccountName = w.account_name,
                Status = w.status,
                CreatedAt = w.created_at,
                ProcessedAt = w.processed_at
            })
            .ToListAsync(ct);

    public void Add(wallet_withdrawal entity) => _db.wallet_withdrawals.Add(entity);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
