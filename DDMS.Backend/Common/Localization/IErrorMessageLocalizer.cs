namespace DDMS.Backend.Common.Localization;

public interface IErrorMessageLocalizer
{
    string Localize(string messageOrKey);

    Dictionary<string, List<string>>? LocalizeFieldErrors(Dictionary<string, List<string>>? fieldErrors);
}
