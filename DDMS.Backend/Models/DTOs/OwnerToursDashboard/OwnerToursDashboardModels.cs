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
    public string TourName { get; set; } = null!;
    public string BoatName { get; set; } = null!;
    public Guid BoatId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
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
