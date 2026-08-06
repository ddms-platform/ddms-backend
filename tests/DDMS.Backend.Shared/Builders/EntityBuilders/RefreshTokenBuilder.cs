using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

/// <summary>Builder cho <see cref="refresh_token"/> — mặc định còn hạn, chưa bị revoke.</summary>
public class RefreshTokenBuilder
{
    private Guid _id = TestGuids.RefreshTokenId;
    private Guid _userId = TestGuids.UserId;
    private string _tokenHash = "hashed-token-value";
    private DateTime _expiresAt = DateTime.UtcNow.AddDays(7);
    private bool _revoked;
    private user? _user;

    public RefreshTokenBuilder WithUserId(Guid userId) { _userId = userId; return this; }
    public RefreshTokenBuilder WithTokenHash(string tokenHash) { _tokenHash = tokenHash; return this; }
    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAtUtc) { _expiresAt = expiresAtUtc; return this; }
    public RefreshTokenBuilder Expired() { _expiresAt = DateTime.UtcNow.AddMinutes(-5); return this; }
    public RefreshTokenBuilder Revoked() { _revoked = true; return this; }
    public RefreshTokenBuilder WithUser(user user) { _user = user; _userId = user.id; return this; }

    public refresh_token Build() => new()
    {
        id = _id,
        user_id = _userId,
        token_hash = _tokenHash,
        expires_at = _expiresAt,
        revoked = _revoked,
        created_at = DateTime.UtcNow.AddDays(-1),
        user = _user ?? new UserBuilder().WithId(_userId).Build()
    };
}
