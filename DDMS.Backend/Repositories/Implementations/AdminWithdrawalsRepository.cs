using DDMS.Backend.Data;
using DDMS.Backend.Models.DTOs.AdminWithdrawals;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class AdminWithdrawalsRepository : IAdminWithdrawalsRepository
{
    private readonly AppDbContext _db;
    public AdminWithdrawalsRepository(AppDbContext db) => _db = db;

    public Task<List<WithdrawalItem>> GetAllAsync(CancellationToken ct) =>
        _db.wallet_withdrawals
            .Include(w => w.user)
            .OrderByDescending(w => w.created_at)
            .Select(w => new WithdrawalItem
            {
                Id = w.id,
                UserId = w.user_id,
                UserFullName = w.user.full_name,
                UserEmail = w.user.email,
                Amount = w.amount,
                BankName = w.bank_name,
                AccountNumber = w.account_number,
                AccountName = w.account_name,
                Status = w.status,
                CreatedAt = w.created_at,
                ProcessedAt = w.processed_at
            })
            .ToListAsync(ct);

    public Task<wallet_withdrawal?> FindWithUserAsync(Guid id, CancellationToken ct) =>
        _db.wallet_withdrawals.Include(w => w.user).FirstOrDefaultAsync(w => w.id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
