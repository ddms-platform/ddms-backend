using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Models.DTOs.Auth;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DDMS.Backend.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;

    public AuthController(IAuthService authService, IGoogleAuthService googleAuthService)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<RegisterResponse>.Ok(result));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request);
        return Ok(ApiResponse<VerifyEmailResponse>.Ok(result));
    }

    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
    {
        var result = await _authService.ResendVerificationEmailAsync(request);
        return Ok(ApiResponse<MessageResponse>.Ok(result));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse<MessageResponse>.Ok(result));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<ResetPasswordResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _authService.ChangePasswordAsync(GetCurrentUserId(), request);
        return Ok(ApiResponse<MessageResponse>.Ok(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, GetIpAddress(), GetUserAgent());
        return Ok(ApiResponse<AuthTokensResponse>.Ok(result));
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _googleAuthService.LoginWithGoogleAsync(request, GetIpAddress(), GetUserAgent());
        return Ok(ApiResponse<AuthTokensResponse>.Ok(result));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request, GetIpAddress(), GetUserAgent());
        return Ok(ApiResponse<AuthTokensResponse>.Ok(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _authService.LogoutAsync(request);
        return Ok(ApiResponse<object>.Ok(new { loggedOut = true }));
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = GetCurrentUserId();
        await _authService.LogoutAllAsync(userId);
        return Ok(ApiResponse<object>.Ok(new { loggedOut = true }));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var result = await _authService.GetMeAsync(GetCurrentUserId());
        return Ok(ApiResponse<CurrentUserResponse>.Ok(result));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException();
        }

        return userId;
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return HttpContext.Request.Headers.UserAgent.ToString();
    }
}
