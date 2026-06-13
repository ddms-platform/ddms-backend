using System;
using System.Threading.Tasks;
using DDMS.Backend.Models.DTOs.Auth;

namespace DDMS.Backend.Services.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<AuthTokensResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
    Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);
    Task<MessageResponse> ResendVerificationEmailAsync(ResendVerificationEmailRequest request);
    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<MessageResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<AuthTokensResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent);
    Task LogoutAsync(LogoutRequest request);
    Task LogoutAllAsync(Guid userId);
    Task<CurrentUserResponse> GetMeAsync(Guid userId);
    Task<MessageResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<MessageResponse> UpdateAvatarAsync(Guid userId, Microsoft.AspNetCore.Http.IFormFile file);
}
