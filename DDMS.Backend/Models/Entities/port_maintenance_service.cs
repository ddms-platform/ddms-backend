using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDMS.Backend.Models.Entities;

[Table("port_maintenance_service")]
public partial class port_maintenance_service
{
    [Key]
    public Guid id { get; set; }

    [Required]
    [StringLength(255)]
    public string name { get; set; } = null!;

    [StringLength(100)]
    public string icon_code { get; set; } = null!;

    public decimal? price { get; set; }

    public string? description { get; set; }

    public DateTime created_at { get; set; } = DateTime.UtcNow;
}
