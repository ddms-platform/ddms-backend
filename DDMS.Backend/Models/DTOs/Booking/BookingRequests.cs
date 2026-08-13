namespace DDMS.Backend.Models.DTOs.Booking;

public class CreateBookingRequest
{
    public Guid ScheduleId { get; set; }
    public int NumPeople { get; set; }

    /// <summary>Mã giảm giá khách nhập. Server tự tra và tự tính mức giảm.</summary>
    public string? PromotionCode { get; set; }

    public string? Notes { get; set; }
    public List<CreateBookingCabinRequest>? Cabins { get; set; }
    public List<CreateBookingServiceRequest>? Services { get; set; }

    // Các field giá dưới đây không còn được dùng: server tự tính toàn bộ từ dữ liệu trong DB.
    // Giữ lại để client cũ gửi lên không lỗi deserialize, nhưng giá trị bị bỏ qua hoàn toàn.
    public Guid? PromotionId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CabinPrice { get; set; }
    public decimal ServicePrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateBookingCabinRequest
{
    public Guid CabinId { get; set; }
    public int Quantity { get; set; }

    /// <summary>Bị bỏ qua — đơn giá lấy từ DB.</summary>
    public decimal UnitPrice { get; set; }
}

public class CreateBookingServiceRequest
{
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }

    /// <summary>Bị bỏ qua — đơn giá lấy từ DB.</summary>
    public decimal UnitPrice { get; set; }
}

public class CheckInBookingRequest
{
    /// <summary>Booking GUID hoặc mã vé 8 ký tự trên QR.</summary>
    public string BookingCode { get; set; } = null!;
}
