namespace DDMS.Backend.Models.DTOs.Booking;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public int NumPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class UserBookingListItemResponse
{
    public string Id { get; set; } = null!;
    public string TourId { get; set; } = null!;
    public string TourTitle_vn { get; set; } = null!;
    public string TourTitle_en { get; set; } = null!;
    public string Location_vn { get; set; } = null!;
    public string Location_en { get; set; } = null!;
    public string Image { get; set; } = null!;
    public string Date { get; set; } = null!;
    public string Time { get; set; } = null!;
    public int Guests { get; set; }
    public double TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public string BookingCode { get; set; } = null!;
    public bool CanShowCheckInQr { get; set; }
    public string CreatedAt { get; set; } = null!;
}

public class CancelBookingResult
{
    public bool Success { get; set; } = true;
    public string Status { get; set; } = null!;
    public bool Refunded { get; set; }
    public decimal AmountRefunded { get; set; }
}

public class CheckInBookingResponse
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string TourName { get; set; } = null!;
    public string BoatName { get; set; } = null!;
    public int NumPeople { get; set; }
    public string DepartureTime { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CheckedInAt { get; set; }
}

public class CabinAvailabilityResponse
{
    public Guid CabinId { get; set; }
    public string CabinName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public int TotalRooms { get; set; }
    public int BookedRooms { get; set; }
    public int AvailableRooms { get; set; }
}
