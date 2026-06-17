using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace DDMS.Backend.Common.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated == true;

    public Guid Id => IdOrNull ?? throw new UnauthorizedException();

    public Guid? IdOrNull
    {
        get
        {
            var raw = _principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? _principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsInRole(string role) => _principal?.IsInRole(role) == true;

    public string? FindClaim(string type) => _principal?.FindFirstValue(type);
}
