namespace DDMS.Backend.Models.DTOs.Billing;

public class FinancialSummaryResponse
{
    public decimal TotalBookingRevenue { get; set; }
    public decimal TotalOwed { get; set; }
    public decimal CommissionOwed { get; set; }
    public decimal MaintenanceOwed { get; set; }
    public decimal DockRentalOwed { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public List<BookingRevenueItem> Bookings { get; set; } = new();
    public List<MaintenanceFeeItem> Maintenances { get; set; } = new();
    public List<DockRentalItem> DockRentals { get; set; } = new();
    public List<PaymentHistoryItem> PaymentHistory { get; set; } = new();
}

public class BookingRevenueItem
{
    public Guid BookingId { get; set; }
    public string TourName { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public DateTime BookingDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public decimal Commission { get; set; }
}

public class MaintenanceFeeItem
{
    public Guid MaintenanceId { get; set; }
    public string BoatName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public decimal Amount { get; set; }
}

public class DockRentalItem
{
    public Guid BoatId { get; set; }
    public string BoatName { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentHistoryItem
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public long PayosOrderCode { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class PaymentInitResult
{
    public string CheckoutUrl { get; set; } = null!;
    public long OrderCode { get; set; }
    public string? QrCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? Bin { get; set; }
}

public record WebhookHandleResult(string Code, string Desc, bool IsSuccess);
