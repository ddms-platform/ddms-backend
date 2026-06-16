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

        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IBillingService, BillingService>();

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddScoped<IWalletRepository, WalletRepository>();

        services.AddScoped<IOwnerToursDashboardRepository, OwnerToursDashboardRepository>();
        services.AddScoped<IOwnerToursDashboardService, OwnerToursDashboardService>();

        services.AddScoped<IOwnerServicesRegistrationRepository, OwnerServicesRegistrationRepository>();
        services.AddScoped<IOwnerServicesRegistrationService, OwnerServicesRegistrationService>();

        services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        services.AddScoped<IAdminOwnersRepository, AdminOwnersRepository>();
        services.AddScoped<IAdminOwnersService, AdminOwnersService>();

        services.AddScoped<IBoatMaintenanceRepository, BoatMaintenanceRepository>();
        services.AddScoped<IBoatMaintenanceService, BoatMaintenanceService>();

        services.AddScoped<ISystemRepository, SystemRepository>();
        services.AddScoped<ISystemService, SystemService>();

        services.AddScoped<IAdminMaintenancesRepository, AdminMaintenancesRepository>();
        services.AddScoped<IAdminMaintenancesService, AdminMaintenancesService>();
        return services;
    }
}
