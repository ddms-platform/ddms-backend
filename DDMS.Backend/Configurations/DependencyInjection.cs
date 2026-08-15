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

        services.AddScoped<IPublicOwnersRepository, PublicOwnersRepository>();
        services.AddScoped<IPublicOwnersService, PublicOwnersService>();

        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IBlogRealtimePublisher, BlogRealtimePublisher>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddHttpClient<IGeminiTextGenerator, GeminiTextGenerator>();
        services.AddHttpClient<IBlogCrawlerService, BlogCrawlerService>();

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingPricingService, BookingPricingService>();

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

        services.AddScoped<IMaintenanceServicesRepository, MaintenanceServicesRepository>();
        services.AddScoped<IMaintenanceServicesService, MaintenanceServicesService>();

        services.AddScoped<IPromotionsRepository, PromotionsRepository>();
        services.AddScoped<IOwnerPromotionsService, OwnerPromotionsService>();
        services.AddScoped<IAdminPromotionsService, AdminPromotionsService>();

        services.AddScoped<IAdminWithdrawalsRepository, AdminWithdrawalsRepository>();
        services.AddScoped<IAdminWithdrawalsService, AdminWithdrawalsService>();

        services.AddScoped<IWithdrawalsRepository, WithdrawalsRepository>();
        services.AddScoped<IWalletService, WalletService>();

        services.AddScoped<IBoatCertificateRepository, BoatCertificateRepository>();
        services.AddScoped<IBoatCertificateService, BoatCertificateService>();
        services.AddScoped<IBoatComplianceService, BoatComplianceService>();
        services.AddScoped<IBoatComplianceNotifier, BoatComplianceNotifier>();
        services.AddScoped<ICertificateTypeRepository, CertificateTypeRepository>();
        services.AddScoped<ICertificateTypeService, CertificateTypeService>();
        services.AddScoped<IOwnerDocumentRepository, OwnerDocumentRepository>();
        services.AddScoped<IOwnerDocumentService, OwnerDocumentService>();

        return services;
    }
}
