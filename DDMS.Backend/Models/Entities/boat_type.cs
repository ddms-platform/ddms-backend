using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDMS.Backend.Models.Entities;

public partial class boat_type
{
    [Key]
    public int id { get; set; }

    [Required]
    [StringLength(50)]
    public string code { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string name_vi { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string name_en { get; set; } = null!;
}
