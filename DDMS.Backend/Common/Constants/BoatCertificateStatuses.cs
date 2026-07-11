namespace DDMS.Backend.Common.Constants;

public static class BoatCertificateStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Expired = "expired";
}

public static class BoatCertificateTypes
{
    public const string Registration = "registration";
    public const string Insurance = "insurance";
    /// <summary>Deprecated: migrated to owner <c>transport_license</c>. Soft-disabled in certificate_types.</summary>
    public const string BusinessLicense = "business_license";
    public const string SafetyCert = "safety_cert";
    public const string CrewCertificate = "crew_certificate";
    public const string Other = "other";
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
