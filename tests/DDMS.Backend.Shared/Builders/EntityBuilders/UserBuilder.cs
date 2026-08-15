using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

/// <summary>Builder cho entity <see cref="user"/> — mặc định là 1 user active, đã verify email, có mật khẩu hợp lệ.</summary>
public class UserBuilder
{
    private Guid _id = TestGuids.UserId;
    private string _fullName = "Nguyen Van A";
    private string _email = "user@example.com";
    private string? _passwordHash = BCrypt.Net.BCrypt.HashPassword("Password1!");
    private string? _phone;
    private string? _address;
    private string? _avatarUrl;
    private bool? _isActive = true;
    private string? _googleId;
    private DateTime? _emailVerifiedAt = DateTime.UtcNow.AddDays(-30);
    private readonly List<user_role> _userRoles = new();
    private owner_profile? _ownerProfile;

    public UserBuilder WithId(Guid id) { _id = id; return this; }
    public UserBuilder WithFullName(string fullName) { _fullName = fullName; return this; }
    public UserBuilder WithEmail(string email) { _email = email; return this; }

    public UserBuilder WithPassword(string plainPassword)
    {
        _passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        return this;
    }

    public UserBuilder WithPasswordHash(string? passwordHash) { _passwordHash = passwordHash; return this; }
    public UserBuilder WithNoPassword() { _passwordHash = null; return this; }
    public UserBuilder WithGoogleId(string? googleId) { _googleId = googleId; return this; }
    public UserBuilder WithPhone(string? phone) { _phone = phone; return this; }
    public UserBuilder WithAddress(string? address) { _address = address; return this; }
    public UserBuilder WithAvatarUrl(string? avatarUrl) { _avatarUrl = avatarUrl; return this; }
    public UserBuilder WithActive(bool isActive) { _isActive = isActive; return this; }

    public UserBuilder WithEmailVerified(bool verified)
    {
        _emailVerifiedAt = verified ? DateTime.UtcNow.AddDays(-30) : null;
        return this;
    }

    public UserBuilder WithRoles(params string[] roleNames)
    {
        _userRoles.Clear();
        var roleId = 1;
        foreach (var roleName in roleNames)
        {
            _userRoles.Add(new user_role
            {
                user_id = _id,
                role_id = roleId,
                assigned_at = DateTime.UtcNow.AddDays(-30),
                role = new role { id = roleId, name = roleName, created_at = DateTime.UtcNow.AddDays(-365) }
            });
            roleId++;
        }
        return this;
    }

    public UserBuilder WithOwnerProfile(bool hasOwnerProfile = true)
    {
        _ownerProfile = hasOwnerProfile
            ? new owner_profile
            {
                id = Guid.NewGuid(),
                user_id = _id,
                entity_type = "individual",
                status = "Verified",
                is_verified = true,
                created_at = DateTime.UtcNow.AddDays(-10),
                updated_at = DateTime.UtcNow.AddDays(-10)
            }
            : null;
        return this;
    }

    public user Build() => new()
    {
        id = _id,
        full_name = _fullName,
        email = _email,
        password_hash = _passwordHash,
        phone = _phone,
        address = _address,
        avatar_url = _avatarUrl,
        is_active = _isActive,
        google_id = _googleId,
        email_verified_at = _emailVerifiedAt,
        created_at = DateTime.UtcNow.AddDays(-30),
        updated_at = DateTime.UtcNow.AddDays(-30),
        user_roles = _userRoles,
        owner_profile = _ownerProfile
    };
}
