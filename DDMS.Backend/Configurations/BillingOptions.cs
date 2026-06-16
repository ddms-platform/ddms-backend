namespace DDMS.Backend.Configurations;

public class BillingOptions
{
    public const string SectionName = "Billing";

    public decimal Commission { get; set; } = 0.08m;
    public decimal MonthlyDockRental { get; set; } = 5_000_000m;
    public string[] RevenueRelevantBookingStatuses { get; set; } = { "confirmed", "paid", "completed" };
    public DateTime OrderCodeEpoch { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public string PayOSReturnUrl { get; set; } = "http://localhost:5173/owner/revenue?payment=success";
    public string PayOSCancelUrl { get; set; } = "http://localhost:5173/owner/revenue?payment=cancel";
}
