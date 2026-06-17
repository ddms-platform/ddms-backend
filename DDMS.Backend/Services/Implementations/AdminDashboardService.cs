using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.AdminDashboard;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class AdminDashboardService : IAdminDashboardService
{
    private const int RecentBookingsTake = 5;
    private const int PendingOwnersTake = 3;
    private const int RevenueBarMonths = 12;

    private readonly IAdminDashboardRepository _repo;
    public AdminDashboardService(IAdminDashboardRepository repo) => _repo = repo;

    public async Task<AdminDashboardResponse> GetStatsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        var totalUsers = await _repo.CountUsersAsync(ct);
        var usersLastMonth = await _repo.CountUsersBeforeAsync(startOfThisMonth, ct);

        var activeTours = await _repo.CountToursByStatusAsync(DashboardBuckets.ActiveTourStatuses, ct);
        if (activeTours == 0) activeTours = await _repo.CountAllToursAsync(ct);
        var toursLastMonth = await _repo.CountToursBeforeAsync(startOfThisMonth, ct);

        var idleBoats = await _repo.CountBoatsByStatusAsync(DashboardBuckets.IdleBoatStatuses, ct);
        if (idleBoats == 0) idleBoats = await _repo.CountActiveBoatsAsync(ct);
        var totalBoats = await _repo.CountActiveBoatsAsync(ct);
        var boatsLastMonth = await _repo.CountActiveBoatsBeforeAsync(startOfThisMonth, ct);

        var revenueThisMonth = await _repo.SumRevenueAsync(DashboardBuckets.CompletedBookingStatuses, startOfThisMonth, null, ct);
        var revenueLastMonth = await _repo.SumRevenueAsync(DashboardBuckets.CompletedBookingStatuses, startOfLastMonth, startOfThisMonth, ct);

        var completed = await _repo.CountBookingsByStatusAsync(DashboardBuckets.CompletedBookingStatuses, ct);
        var pending   = await _repo.CountBookingsByStatusAsync(DashboardBuckets.PendingBookingStatuses, ct);
        var cancelled = await _repo.CountBookingsByStatusAsync(DashboardBuckets.CancelledBookingStatuses, ct);
        var ongoing   = await _repo.CountBookingsByStatusAsync(DashboardBuckets.OngoingBookingStatuses, ct);
        var totalBookings = completed + pending + cancelled + ongoing;

        var recent = await _repo.GetRecentBookingsAsync(RecentBookingsTake, ct);
        var pendingOwners = await _repo.GetPendingOwnerProfilesAsync(DashboardBuckets.PendingOwnerStatuses, PendingOwnersTake, ct);
        var revenueBars = await BuildRevenueBarsAsync(now, ct);
        var systemStats = await BuildSystemStatsAsync(now, ct);

        return new AdminDashboardResponse
        {
            Stats = new List<DashboardStatItem>
            {
                BuildStat("Tổng người dùng", totalUsers.ToString("N0"),
                    PercentChange(totalUsers, usersLastMonth), DashboardColors.StatUsers, DashboardColors.BgUsers),
                BuildStat("Tour đang hoạt động", activeTours.ToString("N0"),
                    PercentChange(activeTours, toursLastMonth), DashboardColors.StatTours, DashboardColors.BgTours),
                BuildStat("Thuyền đang rảnh", idleBoats.ToString("N0"),
                    PercentChange(totalBoats, boatsLastMonth), DashboardColors.StatBoats, DashboardColors.BgBoats),
                BuildStat("Doanh thu tháng này", "₫ " + (revenueThisMonth / 1_000_000m).ToString("0") + "M",
                    PercentChange((double)revenueThisMonth, (double)revenueLastMonth), DashboardColors.StatRevenue, DashboardColors.BgRevenue),
            },
            BookingStatus = new List<BookingStatusItem>
            {
                BuildBucket("Hoàn thành", completed, DashboardColors.Completed, totalBookings),
                BuildBucket("Chờ xử lý",  pending,   DashboardColors.Pending,   totalBookings),
                BuildBucket("Đã hủy",     cancelled, DashboardColors.Cancelled, totalBookings),
                BuildBucket("Đang diễn ra", ongoing, DashboardColors.Ongoing,   totalBookings),
            },
            RecentBookings = recent.Select(MapRecentBooking).ToList(),
            PendingVerify = pendingOwners.Select(MapPendingVerify).ToList(),
            RevenueBars = revenueBars.Select(r => r.Amount).ToList(),
            Months = revenueBars.Select(r => r.Label).ToList(),
            SystemStats = systemStats,
            TotalBookingsThisMonth = totalBookings
        };
    }

    public async Task<List<TopTourItem>> GetTopToursAsync(CancellationToken ct)
    {
        var tours = await _repo.GetAllToursWithCreatorAsync(ct);
        var items = new List<TopTourItem>();
        foreach (var t in tours)
        {
            var bookings = await _repo.CountCompletedBookingsForTourAsync(t.id, ct);
            var revenue = await _repo.SumCompletedRevenueForTourAsync(t.id, ct);
            var profile = t.created_by is Guid creatorId
                ? await _repo.FindOwnerProfileByUserAsync(creatorId, ct)
                : null;

            items.Add(new TopTourItem
            {
                Id = t.id,
                Name = t.name,
                Owner = profile?.business_name ?? t.created_byNavigation?.full_name ?? "Chủ thuyền",
                Bookings = bookings,
                Revenue = (double)revenue,
                AvgRating = (double)t.avg_rating
            });
        }

        return items
            .OrderByDescending(x => x.Bookings)
            .ThenByDescending(x => x.Revenue)
            .Select((x, i) => { x.Rank = i + 1; return x; })
            .ToList();
    }

    private async Task<List<(decimal Amount, string Label)>> BuildRevenueBarsAsync(DateTime now, CancellationToken ct)
    {
        var bars = new List<(decimal, string)>();
        for (var i = RevenueBarMonths - 1; i >= 0; i--)
        {
            var target = now.AddMonths(-i);
            var start = new DateTime(target.Year, target.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            var monthly = await _repo.SumRevenueAsync(DashboardBuckets.CompletedBookingStatuses, start, end, ct);
            bars.Add((Math.Round(monthly / 1_000_000m, 1), "T" + target.Month));
        }
        return bars;
    }

    private async Task<DashboardSystemStats> BuildSystemStatsAsync(DateTime now, CancellationToken ct)
    {
        return new DashboardSystemStats
        {
            TotalDocks = await SafeCountAsync(() => _repo.CountDocksAsync(ct)),
            ActivePromotions = await SafeCountAsync(() => _repo.CountActivePromotionsAsync(now, ct)),
            TodayAuditLogs = await SafeCountAsync(() => _repo.CountAuditLogsSinceAsync(now.Date, ct))
        };
    }

    private static async Task<int> SafeCountAsync(Func<Task<int>> work)
    {
        try { return await work(); } catch { return 0; }
    }

    private static DashboardStatItem BuildStat(string label, string value, double pct, string color, string bg) => new()
    {
        Label = label,
        Value = value,
        Change = FormatPct(pct),
        Up = pct >= 0,
        Color = color,
        Bg = bg
    };

    private static BookingStatusItem BuildBucket(string label, int value, string color, int total) => new()
    {
        Label = label,
        Value = value,
        Color = color,
        Pct = total > 0 ? (int)Math.Round((double)value / total * 100) : 0
    };

    private static RecentBookingItem MapRecentBooking(Models.Entities.booking b) => new()
    {
        Id = b.id.ToString().Substring(0, 8).ToUpper(),
        Customer = b.user?.full_name ?? "Guest",
        Tour = b.schedule?.tour?.name ?? "Tour Service",
        Amount = "₫ " + b.total_price.ToString("N0"),
        Status = b.status,
        Date = b.created_at.ToString("dd/MM/yyyy")
    };

    private static PendingVerifyItem MapPendingVerify(Models.Entities.owner_profile op) => new()
    {
        Name = op.business_name ?? op.user?.full_name ?? "Owner Request",
        License = op.license_number ?? "N/A",
        Ago = FormatAgo(op.created_at)
    };

    private static double PercentChange(double current, double prev) =>
        prev > 0 ? (current - prev) / prev * 100 : 0;
    private static double PercentChange(int current, int prev) => PercentChange((double)current, (double)prev);
    private static string FormatPct(double pct) => (pct >= 0 ? "+" : "") + pct.ToString("0.0") + "%";

    private static string FormatAgo(DateTime date)
    {
        var diff = DateTime.UtcNow - date;
        if (diff.TotalDays >= 7) return $"{(int)(diff.TotalDays / 7)} tuần trước";
        if (diff.TotalDays >= 1) return $"{(int)diff.TotalDays} ngày trước";
        if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours} giờ trước";
        return "Vừa xong";
    }
}
