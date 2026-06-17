using Microsoft.OpenApi;

namespace DDMS.Backend.Extensions;

public static class SwaggerExtensions
{
    private const string BearerSchemeId = "Bearer";

    public static IServiceCollection AddDdmsSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Avoid schemaId collisions when DTOs share the same class name in different namespaces.
            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BoatTour API",
                Version = "v1",
                Description =
                    "Use **Authorize** with the JWT from POST /api/auth/login (paste access token only)."
            });

            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".").Replace("DDMS.Backend.Models.DTOs.", "") ?? type.Name);

            options.AddSecurityDefinition(BearerSchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT from login. Paste the access token (eyJ...); Bearer prefix is added automatically."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeId, document)] = []
            });

            options.EnableAnnotations();
        });

        return services;
    }
}
