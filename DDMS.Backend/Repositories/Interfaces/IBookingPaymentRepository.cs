using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IBookingPaymentRepository
{
    /// <summary>Lần thanh toán đang chờ gần nhất của booking, nếu còn.</summary>
    Task<booking_payment?> FindPendingByBookingAsync(Guid bookingId, CancellationToken ct);

    /// <summary>Tra theo mã đơn PayOS — đường vào của webhook.</summary>
    Task<booking_payment?> FindByOrderCodeAsync(long orderCode, CancellationToken ct);

    Task<booking_payment?> FindLatestByBookingAsync(Guid bookingId, CancellationToken ct);

    /// <summary>Tổng số tiền PayOS xác nhận đã nhận cho booking này.</summary>
    Task<decimal> GetPaidAmountAsync(Guid bookingId, CancellationToken ct);

    /// <summary>Booking đã từng đi qua luồng thanh toán mới hay chưa (đơn cũ thì chưa).</summary>
    Task<bool> HasAnyPaymentAsync(Guid bookingId, CancellationToken ct);

    void Add(booking_payment entity);

    Task SaveChangesAsync(CancellationToken ct);
}
