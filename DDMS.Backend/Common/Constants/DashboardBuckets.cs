namespace DDMS.Backend.Common.Constants;

public static class DashboardBuckets
{
    public static readonly string[] ActiveTourStatuses = { "active", "published", "ongoing" };
    public static readonly string[] IdleBoatStatuses = { "idle", "Rỗi" };

    public static readonly string[] CompletedBookingStatuses = { "completed" };
    public static readonly string[] PendingBookingStatuses = { "pending", "pending_payment", "pending_verification" };
    public static readonly string[] CancelledBookingStatuses = { "cancelled", "rejected" };
    public static readonly string[] OngoingBookingStatuses = { "ongoing", "approved", "confirmed" };

    public static readonly string[] PendingOwnerStatuses = { "pending", "pending_verification" };
}

public static class DashboardColors
{
    public const string Completed = "#10B981";
    public const string Pending   = "#F59E0B";
    public const string Cancelled = "#EF4444";
    public const string Ongoing   = "#3B82F6";

    public const string StatUsers   = "#FF385C";
    public const string StatTours   = "#10B981";
    public const string StatBoats   = "#F59E0B";
    public const string StatRevenue = "#8B5CF6";

    public const string BgUsers   = "rgba(255,56,92,0.12)";
    public const string BgTours   = "rgba(16,185,129,0.12)";
    public const string BgBoats   = "rgba(245,158,11,0.12)";
    public const string BgRevenue = "rgba(139,92,246,0.12)";
}
