using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.RequestBuilders;

public class CreateBookingRequestBuilder
{
    private Guid _scheduleId = TestGuids.ScheduleId;
    private int _numPeople = 2;
    private decimal _totalPrice = 600_000m;
    private List<CreateBookingCabinRequest>? _cabins;

    public CreateBookingRequestBuilder WithScheduleId(Guid scheduleId) { _scheduleId = scheduleId; return this; }
    public CreateBookingRequestBuilder WithNumPeople(int numPeople) { _numPeople = numPeople; return this; }
    public CreateBookingRequestBuilder WithTotalPrice(decimal totalPrice) { _totalPrice = totalPrice; return this; }

    public CreateBookingRequestBuilder WithCabin(Guid cabinId, int quantity, decimal unitPrice)
    {
        _cabins ??= new List<CreateBookingCabinRequest>();
        _cabins.Add(new CreateBookingCabinRequest { CabinId = cabinId, Quantity = quantity, UnitPrice = unitPrice });
        return this;
    }

    public CreateBookingRequest Build() => new()
    {
        ScheduleId = _scheduleId,
        NumPeople = _numPeople,
        BasePrice = _totalPrice,
        CabinPrice = 0,
        ServicePrice = 0,
        DiscountAmount = 0,
        TotalPrice = _totalPrice,
        Cabins = _cabins,
        Services = null
    };
}
