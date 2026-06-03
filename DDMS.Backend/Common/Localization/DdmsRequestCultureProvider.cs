using Microsoft.AspNetCore.Localization;

namespace DDMS.Backend.Common.Localization;

/// <summary>
/// Maps Accept-Language from the frontend (vn, vi, vi-VN) to culture "vi"; otherwise "en".
/// </summary>
public class DdmsRequestCultureProvider : IRequestCultureProvider
{
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var acceptLanguage = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var primary = acceptLanguage.Split(',')[0].Trim();

        if (primary.Equals("vn", StringComparison.OrdinalIgnoreCase)
            || primary.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult("vi"));
        }

        if (primary.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult("en"));
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }
}
