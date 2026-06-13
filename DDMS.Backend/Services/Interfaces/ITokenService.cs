using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(user user, List<string> roles);
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}
