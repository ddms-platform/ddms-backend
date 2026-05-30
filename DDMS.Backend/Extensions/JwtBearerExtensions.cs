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
            }
        };

        return options;
    }
}
