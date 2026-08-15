namespace DDMS.Backend.Common.Constants;

/// <summary>Trạng thái một lần thanh toán booking qua PayOS.</summary>
public static class BookingPaymentStatuses
{
    public const string Pending = "pending";
    public const string Paid = "paid";
    public const string Cancelled = "cancelled";
    public const string Expired = "expired";

    /// <summary>Không còn dùng được nữa, tạo link mới thì phải tạo bản ghi mới.</summary>
    public static bool IsClosed(string status) =>
        status is Paid or Cancelled or Expired;
}
