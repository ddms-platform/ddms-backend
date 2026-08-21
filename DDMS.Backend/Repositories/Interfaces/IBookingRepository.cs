using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<tour_schedule?> FindScheduleWithTourAsync(Guid scheduleId, CancellationToken ct);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken ct);
    Task<int> CancelExpiredHoldsAsync(DateTime now, string reason, CancellationToken ct);
    Task<List<booking>> GetHoldsNeedingReminderAsync(DateTime now, DateTime remindBefore, string agentRole, CancellationToken ct);
    Task<tour_schedule?> FindScheduleWithCabinsAsync(Guid scheduleId, CancellationToken ct);
    Task<Dictionary<Guid, int>> GetBookedCabinQuantitiesAsync(Guid scheduleId, CancellationToken ct);

    /// <summary>Tổng số khách đã đặt (đang chiếm chỗ) trên một lịch trình.</summary>
    Task<int> GetBookedSeatsAsync(Guid scheduleId, CancellationToken ct);

    /// <summary>Đơn giá cabin theo id, giới hạn trong đúng con tàu của lịch trình.</summary>
    Task<Dictionary<Guid, decimal>> GetCabinPricesAsync(Guid boatId, IReadOnlyCollection<Guid> cabinIds, CancellationToken ct);

    /// <summary>Đơn giá dịch vụ theo id, giới hạn trong đúng con tàu của lịch trình.</summary>
    Task<Dictionary<Guid, decimal>> GetServicePricesAsync(Guid boatId, IReadOnlyCollection<Guid> serviceIds, CancellationToken ct);
    Task<bool> HasActiveBookingForTourDateAsync(
        Guid userId,
        Guid tourId,
        DateTime startOfDay,
        DateTime endOfDay,
        CancellationToken ct);
    void AddBooking(booking entity);
    void AddBookingCabin(booking_cabin entity);
    void AddBookingService(booking_service entity);
    Task<List<booking>> GetUserBookingsAsync(Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithScheduleAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindUserBookingWithDetailsAsync(Guid id, Guid userId, CancellationToken ct);

    /// <summary>Booking theo id, không ràng buộc chủ đơn — webhook PayOS không có ngữ cảnh người dùng.</summary>
    Task<booking?> FindBookingWithDetailsAsync(Guid id, CancellationToken ct);

    /// <summary>Booking kèm các dòng cabin/dịch vụ — cần để tính lại giá khi áp mã giảm giá.</summary>
    Task<booking?> FindUserBookingWithLinesAsync(Guid id, Guid userId, CancellationToken ct);
    Task<booking?> FindBookingForCheckInByIdAsync(Guid id, CancellationToken ct);
    Task<booking?> FindBookingForCheckInByCodeAsync(string codePrefix, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
