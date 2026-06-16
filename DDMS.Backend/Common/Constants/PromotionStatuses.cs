namespace DDMS.Backend.Common.Constants;

public static class PromotionStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

public static class DiscountTypes
{
    public const string DbPercent = "percent";
    public const string ApiPercentage = "percentage";

    public static string ToDb(string apiValue) =>
        apiValue == ApiPercentage ? DbPercent : apiValue;

    public static string ToApi(string dbValue) =>
        dbValue == DbPercent ? ApiPercentage : dbValue;
}

public static class WithdrawalStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}
