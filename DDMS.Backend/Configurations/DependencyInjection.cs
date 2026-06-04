using DDMS.Backend.Repositories.Implementations;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Implementations;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectDependencies(this IServiceCollection services)
    {
        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<ITourScheduleRepository, TourScheduleRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<ITourSearchRepository, TourSearchRepository>();
        services.AddScoped<ITourContentRepository, TourContentRepository>();

        services.AddScoped<ITourService, TourService>();
        services.AddScoped<ITourScheduleService, TourScheduleService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<ITourSearchService, TourSearchService>();
        services.AddScoped<ITourContentService, TourContentService>();
        return services;
    }
}
