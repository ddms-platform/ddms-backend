namespace DDMS.Backend.Models.DTOs.OwnerToursDashboard;

public class CreateScheduleRequest
{
    public Guid BoatId { get; set; }
    public Guid TourId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class UpdateBookingStatusRequest
{
    public string Status { get; set; } = null!;
    public string? CancelReason { get; set; }
}

public class TourStatsItem
{
    public string TourName { get; set; } = null!;
    public int BookingsCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ScheduleListItem
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public string TourName { get; set; } = null!;

    /// <summary>Trạng thái của tour (pending/active/...) — FE cần để biết tour đã lên trang khách chưa.</summary>
    public string TourStatus { get; set; } = null!;
    public string BoatName { get; set; } = null!;
    public Guid BoatId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
}

/// <summary>
/// Một tour chủ thuyền đã đăng ký, kèm thông tin đủ để hiển thị danh sách quản lý:
/// trạng thái duyệt, thuyền khai thác và lịch trình sắp tới.
/// </summary>
public class OwnerTourListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? ServiceType { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<string> BoatNames { get; set; } = new();

    /// <summary>Thuyền đầu tiên khai thác tour — dùng để mở form sửa dịch vụ.</summary>
    public Guid? PrimaryBoatId { get; set; }
    public int ScheduleCount { get; set; }
    public int UpcomingScheduleCount { get; set; }
    public DateTime? NextScheduleAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Lý do Admin từ chối — chỉ có khi status = rejected.</summary>
    public string? RejectionReason { get; set; }
}

public class RecentBookingItem
{
    public Guid Id { get; set; }
    public string BookingId { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string ServiceName { get; set; } = null!;
    public string BoatName { get; set; } = null!;
    public DateTime Time { get; set; }
    public decimal Value { get; set; }
    public string Status { get; set; } = null!;
}

public class OwnerResourcesResponse
{
    public List<OwnerBoatResource> Boats { get; set; } = new();
}

public class OwnerBoatResource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public List<OwnerTourResource> Tours { get; set; } = new();
}

public class OwnerTourResource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
