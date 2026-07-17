using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace DDMS.Backend.Configurations;

public static class ValidationConfiguration
{
    public static IServiceCollection AddRequestValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var fieldErrors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "Invalid value"
                                : error.ErrorMessage)
                            .ToList());

                var response = new ApiErrorResponse
                {
                    code = ErrorCode.AuthValidationFailed,
                    message = ErrorCode.Messages.ValidationFailed,
                    fieldErrors = fieldErrors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
