using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

/// <summary>
/// Builder cho <see cref="booking"/> — dựng sẵn đầy đủ navigation graph (schedule -> tour/boat, user)
/// vì BookingService truy cập trực tiếp booking.schedule.tour.name, booking.user.email, v.v.
/// Mặc định: status = pending, lịch khởi hành sau 10 ngày (an toàn với RefundWindow 2 ngày).
/// </summary>
public class BookingBuilder
{
    private Guid _id = TestGuids.BookingId;
    private Guid _userId = TestGuids.UserId;
    private Guid _scheduleId = TestGuids.ScheduleId;
    private int _numPeople = 2;
    private decimal _totalPrice = 600_000m;
    private string _status = BookingStatuses.Pending;
    private DateTime? _holdExpiredAt;
    private string? _cancelReason;
    private DateTime? _cancelledAt;
    private tour_schedule? _schedule;
    private user? _user;

    public BookingBuilder WithId(Guid id) { _id = id; return this; }
    public BookingBuilder WithUserId(Guid userId) { _userId = userId; return this; }
    public BookingBuilder WithStatus(string status) { _status = status; return this; }
    public BookingBuilder WithTotalPrice(decimal totalPrice) { _totalPrice = totalPrice; return this; }
    public BookingBuilder WithNumPeople(int numPeople) { _numPeople = numPeople; return this; }
    public BookingBuilder WithHoldExpiredAt(DateTime? holdExpiredAtUtc) { _holdExpiredAt = holdExpiredAtUtc; return this; }
    public BookingBuilder WithCancelReason(string? reason) { _cancelReason = reason; return this; }
    public BookingBuilder WithCancelledAt(DateTime? cancelledAtUtc) { _cancelledAt = cancelledAtUtc; return this; }
    public BookingBuilder WithSchedule(tour_schedule schedule) { _schedule = schedule; _scheduleId = schedule.id; return this; }
    public BookingBuilder WithUser(user user) { _user = user; _userId = user.id; return this; }

    /// <summary>Tiện ích: đặt lịch khởi hành cách "now" bao nhiêu ngày (dùng cho test RefundWindow).</summary>
    public BookingBuilder ScheduleDepartingInDays(double days)
    {
        _schedule = new TourScheduleBuilder().WithId(_scheduleId).DepartingInDays(days).Build();
        return this;
    }

    public booking Build() => new()
    {
        id = _id,
        user_id = _userId,
        schedule_id = _scheduleId,
        num_people = _numPeople,
        base_price = _totalPrice,
        total_price = _totalPrice,
        status = _status,
        hold_expired_at = _holdExpiredAt,
        cancel_reason = _cancelReason,
        cancelled_at = _cancelledAt,
        created_at = DateTime.UtcNow.AddDays(-1),
        updated_at = DateTime.UtcNow.AddDays(-1),
        schedule = _schedule ?? new TourScheduleBuilder().WithId(_scheduleId).Build(),
        user = _user ?? new UserBuilder().WithId(_userId).Build()
    };
}
