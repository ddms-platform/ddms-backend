namespace DDMS.Backend.Common.Constants;

public static class TourConstants
{
    public static class Statuses
    {
        public const string Active = "active";
        public const string Inactive = "inactive";

        public static readonly HashSet<string> Allowed = [Active, Inactive];
    }

    public static class CancelPolicies
    {
        public const string Free = "free";
        public const string Partial = "partial";
        public const string NoRefund = "no_refund";

        public static readonly HashSet<string> Allowed = [Free, Partial, NoRefund];
    }

    public static class ScheduleStatuses
    {
        public const string Scheduled = "scheduled";
        public const string Ongoing = "ongoing";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";

        public static readonly HashSet<string> Allowed = [Scheduled, Ongoing, Completed, Cancelled];
    }

    public static class SortFields
    {
        public const string Price = "price";
        public const string Rating = "rating";

        public static readonly HashSet<string> Allowed = [Price, Rating];
    }

    public static class SortOrders
    {
        public const string Asc = "asc";
        public const string Desc = "desc";

        public static readonly HashSet<string> Allowed = [Asc, Desc];
    }
}
