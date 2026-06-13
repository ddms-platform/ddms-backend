namespace DDMS.Backend.Models.DTOs.AdminDashboard;

public class AdminDashboardResponse
{
    public List<DashboardStatItem> Stats { get; set; } = new();
    public List<BookingStatusItem> BookingStatus { get; set; } = new();
    public List<RecentBookingItem> RecentBookings { get; set; } = new();
    public List<PendingVerifyItem> PendingVerify { get; set; } = new();
    public List<decimal> RevenueBars { get; set; } = new();
    public List<string> Months { get; set; } = new();
    public DashboardSystemStats SystemStats { get; set; } = new();
    public int TotalBookingsThisMonth { get; set; }
}

public class DashboardStatItem
{
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Change { get; set; } = null!;
    public bool Up { get; set; }
    public string Color { get; set; } = null!;
    public string Bg { get; set; } = null!;
}

public class BookingStatusItem
{
    public string Label { get; set; } = null!;
    public int Value { get; set; }
    public string Color { get; set; } = null!;
    public int Pct { get; set; }
}

public class RecentBookingItem
{
    public string Id { get; set; } = null!;
    public string Customer { get; set; } = null!;
    public string Tour { get; set; } = null!;
    public string Amount { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Date { get; set; } = null!;
}

public class PendingVerifyItem
{
    public string Name { get; set; } = null!;
    public string License { get; set; } = null!;
    public string Ago { get; set; } = null!;
}

public class DashboardSystemStats
{
    public int TotalDocks { get; set; }
    public int ActivePromotions { get; set; }
    public int TodayAuditLogs { get; set; }
}

public class TopTourItem
{
    public int Rank { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Owner { get; set; } = null!;
    public int Bookings { get; set; }
    public double Revenue { get; set; }
    public double AvgRating { get; set; }
}
