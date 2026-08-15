namespace DDMS.Backend.Models.DTOs.Booking;

/// <summary>Thông tin để dựng màn thanh toán: link PayOS + QR ngân hàng.</summary>
public class BookingPaymentInitResult
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string? QrCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? Bin { get; set; }
    public DateTime? ExpiredAt { get; set; }
}

/// <summary>
/// Kết quả đối chiếu với PayOS. Frontend hỏi endpoint này để biết đã trả tiền chưa,
/// thay vì tự khai báo là đã trả.
/// </summary>
public class BookingPaymentStatusResponse
{
    /// <summary>Trạng thái booking sau khi đối chiếu.</summary>
    public string BookingStatus { get; set; } = string.Empty;

    /// <summary>none | pending | paid | cancelled | expired</summary>
    public string PaymentStatus { get; set; } = "none";

    public bool Paid { get; set; }

    public long? OrderCode { get; set; }

    public decimal AmountPaid { get; set; }
}
