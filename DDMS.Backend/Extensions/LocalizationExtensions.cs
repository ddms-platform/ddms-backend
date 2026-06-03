using DDMS.Backend.Common.Localization;
using Microsoft.AspNetCore.Localization;

namespace DDMS.Backend.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection AddDdmsLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "en", "vi" };
            options.SetDefaultCulture("en")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new DdmsRequestCultureProvider());
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        });

        services.AddScoped<IErrorMessageLocalizer, ErrorMessageLocalizer>();

        return services;
    }
}
