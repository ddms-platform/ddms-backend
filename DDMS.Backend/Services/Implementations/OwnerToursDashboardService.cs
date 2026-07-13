using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerToursDashboard;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class OwnerToursDashboardService : IOwnerToursDashboardService
{
    private const int DefaultTourDurationMinutes = 120;
    private const int RecentBookingsTake = 10;
    private const int CleanSeedDay = 19;
    private const int CleanSeedMonth = 6;

    private static readonly string[] DemoBoatNames =
    {
        "Du Thuyền Ánh Dương", "Cano Biển Ngọc", "Tàu Cao Tốc Hải Âu", "Du Thuyền Hoàng Gia",
        "Thuyền Câu Cá Đại Dương", "Tàu Du Lịch Sao Biển", "Cano Siêu Tốc Cá Mập", "Thuyền Vãn Cảnh Hồ Tây"
    };

    private readonly IOwnerToursDashboardRepository _repo;
    private readonly IWalletRepository _wallets;
    private readonly IEmailSender _email;

    public OwnerToursDashboardService(
        IOwnerToursDashboardRepository repo,
        IWalletRepository wallets,
        IEmailSender email)
    {
        _repo = repo;
        _wallets = wallets;
        _email = email;
    }

    public Task<List<TourStatsItem>> GetStatsAsync(Guid ownerId, CancellationToken ct) =>
        _repo.GetTourStatsAsync(ownerId, ct);

    public Task<List<ScheduleListItem>> GetSchedulesAsync(Guid ownerId, int month, int year, CancellationToken ct) =>
        _repo.GetSchedulesAsync(ownerId, month, year, ct);

    public Task<List<RecentBookingItem>> GetRecentBookingsAsync(Guid ownerId, CancellationToken ct) =>
        _repo.GetRecentBookingsAsync(ownerId, RecentBookingsTake, ct);

    public async Task<OwnerResourcesResponse> GetResourcesAsync(Guid ownerId, CancellationToken ct) =>
        new() { Boats = await _repo.GetOwnerResourcesAsync(ownerId, ct) };

    public async Task CreateScheduleAsync(Guid ownerId, CreateScheduleRequest req, CancellationToken ct)
    {
        var boat = await _repo.FindOwnerBoatAsync(req.BoatId, ownerId, ct)
            ?? throw new AppException(ErrorCode.UncategorizedError,
                "Thuyền không tồn tại hoặc không thuộc sở hữu của bạn!");

        var tour = await _repo.FindTourAsync(req.TourId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Tour not found");

        var duration = tour.duration_minutes > 0 ? tour.duration_minutes : DefaultTourDurationMinutes;
        var start = req.StartTime;
        var end = req.EndTime ?? start.AddMinutes(duration);

        if (end <= start)
            throw new AppException(ErrorCode.UncategorizedError, "Thời gian kết thúc phải sau thời gian bắt đầu!");

        if (await _repo.HasScheduleOverlapAsync(req.BoatId, start, end, ct))
            throw new AppException(ErrorCode.UncategorizedError,
                "Thuyền này đã có lịch trình khác trùng thời gian trong khoảng này!");

        var now = DateTime.UtcNow;
        _repo.AddSchedule(new tour_schedule
        {
            id = Guid.NewGuid(),
            tour_id = req.TourId,
            boat_id = req.BoatId,
            start_time = start,
            end_time = end,
            status = TourScheduleStatuses.Scheduled,
            created_at = now,
            updated_at = now
        });
        await _repo.SaveChangesAsync(ct);
    }

    public async Task<string> UpdateBookingStatusAsync(Guid ownerId, Guid bookingId, UpdateBookingStatusRequest req, CancellationToken ct)
    {
        var booking = await _repo.FindOwnerBookingWithDetailsAsync(bookingId, ownerId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Booking not found or access denied");

        var wasPaid = BookingStatuses.IsPaidLike(booking.status);
        var now = DateTime.UtcNow;

        booking.status = req.Status;
        booking.updated_at = now;

        if (string.Equals(req.Status, BookingStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            booking.cancelled_at = now;
            booking.cancel_reason = req.CancelReason ?? BookingStatuses.CancelReasonOwnerCancelled;
            if (wasPaid) await RefundToWalletAsync(booking.user_id, booking.total_price, ct);
        }

        await _repo.SaveChangesAsync(ct);
        await TrySendStatusEmailAsync(booking, req);

        return req.Status;
    }

    public async Task<string> CleanSeedDataAsync(CancellationToken ct)
    {
        var old = await _repo.GetSchedulesByDayMonthAsync(CleanSeedDay, CleanSeedMonth, ct);
        _repo.RemoveSchedules(old);
        await _repo.SaveChangesAsync(ct);
        return $"Đã xóa các lịch trình cũ bị lỗi ngày {CleanSeedDay}!";
    }

    public async Task<string> RenameBoatsAsync(CancellationToken ct)
    {
        var boats = await _repo.GetAllBoatsOrderedAsync(ct);
        for (var i = 0; i < boats.Count; i++)
            boats[i].name = i < DemoBoatNames.Length ? DemoBoatNames[i] : $"Thuyền phụ {i + 1}";
        await _repo.SaveChangesAsync(ct);
        return $"Renamed {boats.Count} boats successfully";
    }

    public async Task<string> SeedToursAsync(CancellationToken ct)
    {
        var boat = await _repo.GetFirstBoatAsync(ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Chưa có thuyền nào trong DB.");

        var now = DateTime.UtcNow;
        var tours = new List<tour>
        {
            new() { id = Guid.NewGuid(), name = "Đà Nẵng City Tour Khởi Hành Từ Bến", description = "Tour vòng quanh sông Hàn", price = 1250000, duration_minutes = 120, status = TourStatuses.Active, cancel_policy = "free", created_at = now, updated_at = now },
            new() { id = Guid.NewGuid(), name = "Tour Cù Lao Chàm Siêu Tốc", description = "Đi Cù Lao Chàm bằng cano siêu tốc", price = 850000, duration_minutes = 360, status = TourStatuses.Active, cancel_policy = "free", created_at = now, updated_at = now }
        };
        _repo.AddTours(tours);

        var farFuture = now.AddYears(10);
        foreach (var t in tours)
        {
            _repo.AddSchedule(new tour_schedule
            {
                id = Guid.NewGuid(),
                tour_id = t.id,
                boat_id = boat.id,
                start_time = farFuture,
                end_time = farFuture.AddMinutes(DefaultTourDurationMinutes),
                status = TourScheduleStatuses.Scheduled,
                created_at = now,
                updated_at = now
            });
        }

        await _repo.SaveChangesAsync(ct);
        return $"Đã tạo 2 tour mẫu cho thuyền {boat.name}";
    }

    private async Task RefundToWalletAsync(Guid userId, decimal amount, CancellationToken ct)
    {
        var wallet = await _wallets.FindAsync(userId, ct);
        var now = DateTime.UtcNow;
        if (wallet == null)
        {
            wallet = new user_wallet
            {
                id = Guid.NewGuid(),
                user_id = userId,
                balance = 0m,
                created_at = now,
                updated_at = now
            };
            _wallets.Add(wallet);
        }
        wallet.balance += amount;
        wallet.updated_at = now;
    }

    private async Task TrySendStatusEmailAsync(booking booking, UpdateBookingStatusRequest req)
    {
        try
        {
            await _email.SendBookingStatusEmailAsync(
                booking.user.email,
                booking.user.full_name ?? "Khách hàng",
                booking.id.ToString().Substring(0, 8).ToUpper(),
                booking.schedule.tour.name,
                booking.schedule.boat?.name ?? "N/A",
                booking.schedule.start_time,
                booking.total_price,
                req.Status,
                req.CancelReason);
        }
        catch
        {
            // best-effort: nuốt lỗi gửi mail, không fail request
        }
    }
}
