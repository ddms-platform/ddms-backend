using DDMS.Backend.Models.DTOs.Auth;

namespace DDMS.Backend.Models.Services.Interfaces;

public interface IGoogleAuthService
{
    Task<AuthTokensResponse> LoginWithGoogleAsync(GoogleLoginRequest request, string? ipAddress, string? userAgent);
}
