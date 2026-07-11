namespace DDMS.Backend.Common.Constants;

public static class BoatCertificateStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Expired = "expired";
}

/// <summary>
/// Boat-scoped certificate type codes. Prefer active rows in <c>certificate_types</c> (scope=boat).
/// </summary>
public static class BoatCertificateTypes
{
    public const string Registration = "registration";
    public const string Insurance = "insurance";
    public const string SafetyCert = "safety_cert";
    public const string CrewCertificate = "crew_certificate";
    public const string FireSafety = "fire_safety";
    public const string Other = "other";

    /// <summary>
    /// Deprecated boat type — migrated to owner <c>transport_license</c>. Soft-disabled in DB; reject new uploads.
    /// </summary>
    [Obsolete("Migrated to owner transport_license. Soft-disabled in certificate_types.")]
    public const string BusinessLicense = "business_license";

    public static bool IsDeprecated(string? code) =>
        string.Equals(code, "business_license", StringComparison.OrdinalIgnoreCase);
}

public static class BoatComplianceStatuses
{
    public const string Valid = "valid";
    public const string Warning = "warning";
    public const string Hidden = "hidden";
    public const string Locked = "locked";

    public static bool IsBlocked(string? status) =>
        status is Hidden or Locked;
}
