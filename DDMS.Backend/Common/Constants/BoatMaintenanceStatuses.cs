namespace DDMS.Backend.Common.Constants;

public static class BoatMaintenanceStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string DefaultServiceName = "Dịch vụ bảo trì";
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(2);
}
