using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IWalletRepository
{
    Task<user_wallet?> FindAsync(Guid userId, CancellationToken ct);
    void Add(user_wallet wallet);
}
