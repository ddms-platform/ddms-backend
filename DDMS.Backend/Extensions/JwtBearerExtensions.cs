using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using DDMS.Backend.Common.Constants;
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
                    code = isExpired ? ErrorDefinitions.Codes.AuthTokenExpired : ErrorDefinitions.Codes.AuthUnauthorized,
                    message = isExpired ? ErrorDefinitions.Messages.TokenExpired : ErrorDefinitions.Messages.Unauthorized
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(error));
            }
        };

        return options;
    }
}
