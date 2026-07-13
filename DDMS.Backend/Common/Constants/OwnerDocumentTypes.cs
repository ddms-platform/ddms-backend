namespace DDMS.Backend.Common.Constants;

public static class OwnerEntityTypes
{
    public const string Individual = "individual";
    public const string Business = "business";
    public const string Cooperative = "cooperative";

    public static bool IsValid(string? value) =>
        value is Individual or Business or Cooperative;

    public static bool RequiresBusinessRegistration(string? entityType) =>
        entityType is Business or Cooperative;
}

public static class OwnerDocumentTypes
{
    public const string NationalId = "national_id";
    public const string TransportLicense = "transport_license";
    public const string BusinessRegistration = "business_registration";
    public const string ResidenceProof = "residence_proof";
    public const string AuthorizationLetter = "authorization_letter";

    public static IReadOnlyList<string> GetRequiredTypes(string entityType)
    {
        var required = new List<string> { NationalId, TransportLicense };
        if (OwnerEntityTypes.RequiresBusinessRegistration(entityType))
            required.Add(BusinessRegistration);
        return required;
    }
}

public static class CertificateScopes
{
    public const string Boat = "boat";
    public const string Owner = "owner";

    public static bool IsValid(string? value) =>
        value is Boat or Owner;
}
