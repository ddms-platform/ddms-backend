using DDMS.Backend.Common.Constants;
using DDMS.Backend.Data;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Repositories.Implementations;

public class BookingPaymentRepository : IBookingPaymentRepository
{
    private readonly AppDbContext _db;

    public BookingPaymentRepository(AppDbContext db) => _db = db;

    public Task<booking_payment?> FindPendingByBookingAsync(Guid bookingId, CancellationToken ct) =>
        _db.booking_payments
            .Where(p => p.booking_id == bookingId && p.status == BookingPaymentStatuses.Pending)
            .OrderByDescending(p => p.created_at)
            .FirstOrDefaultAsync(ct);

    public Task<booking_payment?> FindByOrderCodeAsync(long orderCode, CancellationToken ct) =>
        _db.booking_payments.FirstOrDefaultAsync(p => p.payos_order_code == orderCode, ct);

    public Task<booking_payment?> FindLatestByBookingAsync(Guid bookingId, CancellationToken ct) =>
        _db.booking_payments
            .Where(p => p.booking_id == bookingId)
            .OrderByDescending(p => p.created_at)
            .FirstOrDefaultAsync(ct);

    public async Task<decimal> GetPaidAmountAsync(Guid bookingId, CancellationToken ct) =>
        await _db.booking_payments
            .Where(p => p.booking_id == bookingId && p.status == BookingPaymentStatuses.Paid)
            .SumAsync(p => (decimal?)p.amount_paid, ct) ?? 0m;

    public Task<bool> HasAnyPaymentAsync(Guid bookingId, CancellationToken ct) =>
        _db.booking_payments.AnyAsync(p => p.booking_id == bookingId, ct);

    public void Add(booking_payment entity) => _db.booking_payments.Add(entity);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
