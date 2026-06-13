using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDMS.Backend.Models.Entities;

[Table("owner_payment")]
public partial class owner_payment
{
    [Key]
    public Guid id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid owner_id { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal amount { get; set; }

    [Required]
    [StringLength(50)]
    public string status { get; set; } = "pending"; // pending, paid, cancelled

    [Required]
    public long payos_order_code { get; set; }

    [StringLength(255)]
    public string? description { get; set; }

    [Required]
    public DateTime created_at { get; set; } = DateTime.UtcNow;

    public DateTime? paid_at { get; set; }

    [ForeignKey("owner_id")]
    public virtual user owner { get; set; } = null!;
}
