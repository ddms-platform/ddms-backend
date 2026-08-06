using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

public class UserWalletBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = TestGuids.UserId;
    private decimal _balance;

    public UserWalletBuilder WithUserId(Guid userId) { _userId = userId; return this; }
    public UserWalletBuilder WithBalance(decimal balance) { _balance = balance; return this; }

    public user_wallet Build() => new()
    {
        id = _id,
        user_id = _userId,
        balance = _balance,
        created_at = DateTime.UtcNow.AddDays(-30),
        updated_at = DateTime.UtcNow.AddDays(-30)
    };
}
