using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Models.Entities;

public partial class certificate_type
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

    public int sort_order { get; set; }

    /// <summary>Applies to boat certificates or owner documents: boat | owner.</summary>
    [Required]
    [StringLength(20)]
    public string scope { get; set; } = "boat";

    public bool is_active { get; set; } = true;
}
