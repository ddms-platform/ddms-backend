namespace DDMS.Backend.Common.Constants;

public static class BillingRates
{
    public const decimal Commission = 0.08m;
    public const decimal MonthlyDockRental = 5_000_000m;

    public static readonly string[] RevenueRelevantBookingStatuses =
        { "confirmed", "paid", "completed" };

    public static readonly DateTime OrderCodeEpoch = new(2026, 1, 1);

    public const string PayOSReturnUrl = "http://localhost:5173/owner/revenue?payment=success";
    public const string PayOSCancelUrl = "http://localhost:5173/owner/revenue?payment=cancel";
}
