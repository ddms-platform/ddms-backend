using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Models.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    public string fullName { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? phone { get; set; }

    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? address { get; set; }
}
