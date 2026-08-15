using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Configurations;

public class BillingOptions
{
    public const string SectionName = "Billing";

    [Range(0, 1, ErrorMessage = "Billing:Commission phải nằm trong [0, 1].")]
    public decimal Commission { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Billing:MonthlyDockRental phải >= 0.")]
    public decimal MonthlyDockRental { get; set; }

    [Required, MinLength(1, ErrorMessage = "Billing:RevenueRelevantBookingStatuses không được rỗng.")]
    public string[] RevenueRelevantBookingStatuses { get; set; } = Array.Empty<string>();

    [Required(ErrorMessage = "Billing:OrderCodeEpoch là bắt buộc.")]
    public DateTime OrderCodeEpoch { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Billing:PayOSReturnUrl là bắt buộc.")]
    [Url(ErrorMessage = "Billing:PayOSReturnUrl không phải URL hợp lệ.")]
    public string PayOSReturnUrl { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Billing:PayOSCancelUrl là bắt buộc.")]
    [Url(ErrorMessage = "Billing:PayOSCancelUrl không phải URL hợp lệ.")]
    public string PayOSCancelUrl { get; set; } = null!;

    /// <summary>Nơi PayOS trả khách về sau khi thanh toán tour thành công.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Billing:PayOSBookingReturnUrl là bắt buộc.")]
    [Url(ErrorMessage = "Billing:PayOSBookingReturnUrl không phải URL hợp lệ.")]
    public string PayOSBookingReturnUrl { get; set; } = null!;

    /// <summary>Nơi PayOS trả khách về khi khách bấm huỷ thanh toán tour.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Billing:PayOSBookingCancelUrl là bắt buộc.")]
    [Url(ErrorMessage = "Billing:PayOSBookingCancelUrl không phải URL hợp lệ.")]
    public string PayOSBookingCancelUrl { get; set; } = null!;
}
