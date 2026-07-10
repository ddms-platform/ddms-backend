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
    public const string BusinessLicense = "business_license";
    public const string SafetyCert = "safety_cert";
    public const string Other = "other";
}

public static class BoatComplianceStatuses
{
    public const string Valid = "valid";
    public const string Warning = "warning";
    public const string Hidden = "hidden";
    public const string Locked = "locked";
}
