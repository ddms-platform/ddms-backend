using Microsoft.OpenApi;

namespace DDMS.Backend.Configurations;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DDMS Backend API",
                Version = "v1",
                Description = "Boat Tour Management System — REST API for tours, schedules, routes, search and content."
            });

            options.EnableAnnotations();
        });

        return services;
    }
}
