namespace DDMS.Backend.Common.Identity;

public interface ICurrentUser
{
    Guid Id { get; }
    Guid? IdOrNull { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    string? FindClaim(string type);
}
