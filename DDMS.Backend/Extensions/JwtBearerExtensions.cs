using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DDMS.Backend.Extensions;

public static class JwtBearerExtensions
{
    public static JwtBearerOptions ConfigureDdmsJwtBearer(this JwtBearerOptions options)
    {
        // Keep JWT short claim names ("role", "sub"). Without this, [Authorize(Roles=...)]
        // looks for ClaimTypes.Role URI and returns 403 even when the token has "role".
        options.MapInboundClaims = false;
        options.TokenValidationParameters.RoleClaimType = "role";
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Sub;

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                if (context.Response.HasStarted)
                {
                    return;
                }

                var isExpired = context.AuthenticateFailure is SecurityTokenExpiredException;
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var error = new ApiErrorResponse
                {
                    code = isExpired ? ErrorCode.AuthTokenExpired : ErrorCode.AuthUnauthorized,
                    message = isExpired ? ErrorCode.Messages.TokenExpired : ErrorCode.Messages.Unauthorized
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(error));
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };

        return options;
    }
}
