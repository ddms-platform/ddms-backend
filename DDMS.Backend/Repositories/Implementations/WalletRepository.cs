using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _db;
    public WalletRepository(AppDbContext db) => _db = db;

    public Task<user_wallet?> FindAsync(Guid userId, CancellationToken ct) =>
        _db.user_wallets.FirstOrDefaultAsync(w => w.user_id == userId, ct);

    public void Add(user_wallet wallet) => _db.user_wallets.Add(wallet);
}
