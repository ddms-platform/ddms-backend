namespace DDMS.Backend.Models.DTOs.Booking;

public class CreateBookingRequest
{
    public Guid ScheduleId { get; set; }
    public Guid? PromotionId { get; set; }
    public int NumPeople { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CabinPrice { get; set; }
    public decimal ServicePrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    public List<CreateBookingCabinRequest>? Cabins { get; set; }
    public List<CreateBookingServiceRequest>? Services { get; set; }
}

public class CreateBookingCabinRequest
{
    public Guid CabinId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateBookingServiceRequest
{
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CheckInBookingRequest
{
    /// <summary>Booking GUID hoặc mã vé 8 ký tự trên QR.</summary>
    public string BookingCode { get; set; } = null!;
}
