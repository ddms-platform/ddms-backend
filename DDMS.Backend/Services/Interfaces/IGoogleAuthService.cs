using DDMS.Backend.Models.DTOs.Auth;

namespace DDMS.Backend.Services.Interfaces;

public interface IGoogleAuthService
{
    Task<AuthTokensResponse> LoginWithGoogleAsync(GoogleLoginRequest request, string? ipAddress, string? userAgent);
}
