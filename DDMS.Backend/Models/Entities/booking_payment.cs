using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDMS.Backend.Models.Entities;

/// <summary>
/// Một lần khách thanh toán cho booking qua PayOS.
/// Booking chỉ được xác nhận khi có bản ghi ở đây với status = "paid",
/// và trạng thái đó chỉ do server ghi sau khi đối chiếu với PayOS.
/// </summary>
[Table("booking_payment")]
public partial class booking_payment
{
    [Key]
    public Guid id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid booking_id { get; set; }

    /// <summary>Số tiền yêu cầu khách trả tại thời điểm tạo link.</summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal amount { get; set; }

    /// <summary>Số tiền PayOS xác nhận đã nhận. Dùng để hoàn tiền, không dùng total_price.</summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal amount_paid { get; set; }

    /// <summary>pending | paid | cancelled | expired</summary>
    [Required]
    [StringLength(50)]
    public string status { get; set; } = "pending";

    [Required]
    public long payos_order_code { get; set; }

    [StringLength(255)]
    public string? description { get; set; }

    [StringLength(500)]
    public string? checkout_url { get; set; }

    [Required]
    public DateTime created_at { get; set; } = DateTime.UtcNow;

    public DateTime? paid_at { get; set; }

    [ForeignKey("booking_id")]
    public virtual booking booking { get; set; } = null!;
}
