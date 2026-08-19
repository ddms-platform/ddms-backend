namespace DDMS.Backend.Models.DTOs.Booking;

/// <summary>
/// Kết quả tính giá một booking, do server tính hoàn toàn từ dữ liệu trong DB.
/// Dùng chung cho cả xem trước mã giảm giá lẫn lúc tạo booking, nên số tiền
/// khách nhìn thấy luôn bằng số tiền thực sự được ghi vào đơn.
/// </summary>
public class BookingQuote
{
    public decimal BasePrice { get; set; }
    public decimal CabinPrice { get; set; }
    public decimal ServicePrice { get; set; }

    /// <summary>Tổng trước giảm giá.</summary>
    public decimal Subtotal { get; set; }

    public Guid? PromotionId { get; set; }
    public string? PromotionCode { get; set; }

    /// <summary>Mô tả mức giảm để hiển thị, ví dụ "Giảm 10%" hoặc "Giảm 50.000đ".</summary>
    public string? PromotionDescription { get; set; }

    public decimal DiscountAmount { get; set; }

    /// <summary>Subtotal trừ đi DiscountAmount, không bao giờ âm.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Cabin kèm đơn giá đã tra từ DB — dùng để ghi booking_cabin.</summary>
    public List<PricedLine> CabinLines { get; set; } = [];

    /// <summary>Dịch vụ kèm đơn giá đã tra từ DB — dùng để ghi booking_service.</summary>
    public List<PricedLine> ServiceLines { get; set; } = [];

    /// <summary>Tiền tour tách theo hạng vé — để hoá đơn hiện rõ từng dòng.
    /// Hạng không có khách thì không xuất hiện.</summary>
    public List<PartyPricedLine> PartyLines { get; set; } = [];
}

/// <summary>Một hạng vé trên hoá đơn.</summary>
public class PartyPricedLine
{
    /// <summary>adult / child / infant — xem <see cref="Common.Constants.PassengerTiers"/>.</summary>
    public string Tier { get; set; } = string.Empty;

    public int Quantity { get; set; }

    /// <summary>Giá một khách của hạng này, đã nhân tỉ lệ.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Thành tiền cả dòng, đã làm tròn. Tổng các dòng chính là BasePrice.</summary>
    public decimal LineTotal { get; set; }
}

/// <summary>Một dòng đã được server gán đơn giá.</summary>
public class PricedLine
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>Áp một mã giảm giá lên booking đang chờ thanh toán.</summary>
public class ApplyPromotionRequest
{
    public string? Code { get; set; }
}

/// <summary>Một dòng cabin hoặc dịch vụ. Đơn giá do server tra, client chỉ nói đặt gì và bao nhiêu.</summary>
public class BookingLineRequest
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}
