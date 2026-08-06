using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

/// <summary>
/// Builder cho <see cref="tour_schedule"/> — mặc định khởi hành sau 10 ngày (đủ xa để không
/// vướng các ngưỡng "NoHoldWithinDays" / "RefundWindow"), có sẵn tour + boat hợp lệ.
/// </summary>
public class TourScheduleBuilder
{
    private Guid _id = TestGuids.ScheduleId;
    private Guid _tourId = TestGuids.TourId;
    private Guid? _boatId = TestGuids.BoatId;
    private DateTime _startTime = DateTime.UtcNow.AddDays(10);
    private DateTime _endTime = DateTime.UtcNow.AddDays(10).AddHours(2);
    private string _status = "active";
    private tour? _tour;
    private boat? _boat = new BoatBuilder().Build();

    public TourScheduleBuilder WithId(Guid id) { _id = id; return this; }
    public TourScheduleBuilder WithStartTime(DateTime startTimeUtc) { _startTime = startTimeUtc; return this; }
    public TourScheduleBuilder WithEndTime(DateTime endTimeUtc) { _endTime = endTimeUtc; return this; }
    public TourScheduleBuilder WithTour(tour tour) { _tour = tour; _tourId = tour.id; return this; }
    public TourScheduleBuilder WithBoat(boat? boat) { _boat = boat; _boatId = boat?.id; return this; }
    public TourScheduleBuilder WithNoBoat() { _boat = null; _boatId = null; return this; }

    /// <summary>Đặt ngày khởi hành cách "now" bao nhiêu ngày — tiện cho test RefundWindow/HoldPolicy.</summary>
    public TourScheduleBuilder DepartingInDays(double days)
    {
        _startTime = DateTime.UtcNow.AddDays(days);
        _endTime = _startTime.AddHours(2);
        return this;
    }

    public tour_schedule Build() => new()
    {
        id = _id,
        tour_id = _tourId,
        boat_id = _boatId,
        start_time = _startTime,
        end_time = _endTime,
        status = _status,
        created_at = DateTime.UtcNow.AddDays(-50),
        updated_at = DateTime.UtcNow.AddDays(-50),
        tour = _tour ?? new TourBuilder().WithId(_tourId).Build(),
        boat = _boat
    };
}
