namespace DDMS.Backend.Common.Constants;

public static class BookingStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Paid = "paid";
    public const string Completed = "completed";
    public const string CheckedIn = "checked_in";
    public const string Cancelled = "cancelled";

    public static readonly TimeSpan RefundWindow = TimeSpan.FromDays(2);

    public const string DefaultTourImage =
        "https://images.unsplash.com/photo-1559592413-7cec4d0cae2b?w=600&h=400&fit=crop";

    public const string DefaultLocationVn = "Đà Nẵng";
    public const string DefaultLocationEn = "Da Nang";

    public const string CancelReasonRefunded = "Hủy bởi khách hàng (Được hoàn tiền)";
    public const string CancelReasonNoRefund = "Hủy bởi khách hàng (Không hoàn tiền - Sát ngày khởi hành < 2 ngày)";
    public const string CancelReasonGeneric = "Hủy bởi khách hàng";

    public static bool IsPaidLike(string status) =>
        status.Equals(Paid, StringComparison.OrdinalIgnoreCase)
     || status.Equals(Confirmed, StringComparison.OrdinalIgnoreCase)
     || status.Equals(CheckedIn, StringComparison.OrdinalIgnoreCase);

    public static bool CanCheckIn(string status) =>
        status.Equals(Paid, StringComparison.OrdinalIgnoreCase)
     || status.Equals(Confirmed, StringComparison.OrdinalIgnoreCase);

    public static string ToFrontendStatus(string dbStatus) => dbStatus switch
    {
        Paid or Confirmed or CheckedIn => "UPCOMING",
        Completed => "COMPLETED",
        Cancelled => "CANCELLED",
        _ => "PENDING"
    };
}
