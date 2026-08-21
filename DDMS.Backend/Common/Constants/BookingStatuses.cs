namespace DDMS.Backend.Common.Constants;

public static class BookingStatuses
{
    public const string Pending = "pending";

    /// <summary>Đang giữ chỗ tạm thời, chờ thanh toán trước khi hết hạn (hold_expired_at).</summary>
    public const string Holding = "holding";

    public const string Confirmed = "confirmed";
    public const string Paid = "paid";
    public const string Completed = "completed";
    public const string CheckedIn = "checked_in";
    public const string Cancelled = "cancelled";

    public static readonly TimeSpan RefundWindow = TimeSpan.FromDays(2);

    /// <summary>
    /// Những trạng thái đơn đang chiếm chỗ (ghế/phòng) trên một lịch trình.
    /// Phải là List chứ không phải mảng: với mảng, .NET 10 chọn overload
    /// MemoryExtensions.Contains(ReadOnlySpan&lt;string&gt;, ...) mà EF không dịch sang SQL được.
    /// </summary>
    public static readonly List<string> OccupyingStatuses = [Pending, Holding, Confirmed, Paid, CheckedIn];

    /// <summary>
    /// Trạng thái được chuyển sang <see cref="Completed"/> khi chuyến đã chạy xong.
    /// Chỉ đơn đã trả tiền — đơn huỷ hoặc chưa thanh toán thì khách đâu có đi.
    /// </summary>
    public static readonly List<string> CompletableStatuses = [Confirmed, Paid, CheckedIn];

    /// <summary>Phiên bản hàm của <see cref="OccupyingStatuses"/>, dùng ở tầng service.</summary>
    public static bool OccupiesInventory(string status) =>
        OccupyingStatuses.Contains(status.ToLowerInvariant());

    public const string DefaultTourImage =
        "https://images.unsplash.com/photo-1559592413-7cec4d0cae2b?w=600&h=400&fit=crop";

    public const string DefaultLocationVn = "Đà Nẵng";
    public const string DefaultLocationEn = "Da Nang";

    public const string CancelReasonRefunded = "Hủy bởi khách hàng (Được hoàn tiền)";
    public const string CancelReasonNoRefund = "Hủy bởi khách hàng (Không hoàn tiền - Sát ngày khởi hành < 2 ngày)";
    public const string CancelReasonGeneric = "Hủy bởi khách hàng";
    public const string CancelReasonHoldExpired = "Tự động huỷ do quá hạn giữ chỗ";
    public const string CancelReasonOwnerCancelled = "Hủy bởi chủ tàu (Hoàn tiền tự động)";

    public static bool IsPaidLike(string status) =>
        status.Equals(Paid, StringComparison.OrdinalIgnoreCase)
     || status.Equals(Confirmed, StringComparison.OrdinalIgnoreCase)
     || status.Equals(CheckedIn, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Check-in eligible ("Pending" in business terms): paid/confirmed, not yet checked in.
    /// </summary>
    public static bool CanCheckIn(string status) =>
        status.Equals(Paid, StringComparison.OrdinalIgnoreCase)
     || status.Equals(Confirmed, StringComparison.OrdinalIgnoreCase);

    public static bool CanShowCheckInQr(string status) => CanCheckIn(status);

    public static bool IsOwnerCancelled(string? cancelReason) =>
        !string.IsNullOrWhiteSpace(cancelReason)
        && cancelReason.Contains("chủ tàu", StringComparison.OrdinalIgnoreCase);

    public static string ToBookingCode(Guid bookingId) =>
        bookingId.ToString()[..8].ToUpperInvariant();

    public static string ToFrontendStatus(string dbStatus) => dbStatus switch
    {
        Paid or Confirmed => "UPCOMING",
        CheckedIn => "CHECKED_IN",
        Completed => "COMPLETED",
        Cancelled => "CANCELLED",
        _ => "PENDING"
    };
}
