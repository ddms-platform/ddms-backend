namespace DDMS.Backend.Common.Constants;

public static class TourScheduleStatuses
{
    public const string Scheduled = "scheduled";
    public const string Cancelled = "cancelled";
    public const string Completed = "completed";

    /// <summary>
    /// Là deny-list chứ không phải allow-list: dữ liệu cũ có nhiều trạng thái khác nhau,
    /// chặn theo danh sách trắng sẽ vô cớ khoá luôn cả những lịch trình đang bán bình thường.
    /// </summary>
    public static bool IsClosedForBooking(string? status) =>
        status is not null
        && (status.Trim().Equals(Cancelled, StringComparison.OrdinalIgnoreCase)
            || status.Trim().Equals(Completed, StringComparison.OrdinalIgnoreCase));
}

public static class TourStatuses
{
    public const string Active = "active";
}
