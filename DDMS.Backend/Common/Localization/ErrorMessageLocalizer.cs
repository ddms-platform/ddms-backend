using DDMS.Backend.Resources;
using Microsoft.Extensions.Localization;

namespace DDMS.Backend.Common.Localization;

public class ErrorMessageLocalizer(IStringLocalizer<TourResources> localizer) : IErrorMessageLocalizer
{
    public string Localize(string messageOrKey)
    {
        if (string.IsNullOrWhiteSpace(messageOrKey))
        {
            return messageOrKey;
        }

        var localized = localizer[messageOrKey];
        return localized.ResourceNotFound ? messageOrKey : localized.Value;
    }

    public Dictionary<string, List<string>>? LocalizeFieldErrors(Dictionary<string, List<string>>? fieldErrors)
    {
        if (fieldErrors is null || fieldErrors.Count == 0)
        {
            return fieldErrors;
        }

        return fieldErrors.ToDictionary(
            static kvp => kvp.Key,
            kvp => kvp.Value.Select(Localize).ToList());
    }
}
