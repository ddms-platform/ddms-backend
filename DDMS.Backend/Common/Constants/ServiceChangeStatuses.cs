namespace DDMS.Backend.Common.Constants;

public static class ServiceChangeStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

public static class ServiceApprovalKinds
{
    /// <summary>Tạo tour mới (kèm phòng/combo) — vào hàng duyệt tour.</summary>
    public const string NewTour = "new_tour";

    /// <summary>Sửa tour chưa từng lên trang bán — vẫn hàng duyệt tour, không tạo tour mới.</summary>
    public const string TourResubmit = "tour_resubmit";

    /// <summary>Sửa dịch vụ của tour đang bán — hàng duyệt dịch vụ, tour live không đổi.</summary>
    public const string ServiceChange = "service_change";
}
