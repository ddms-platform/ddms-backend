using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using DDMS.Backend.Models.DTOs.BoatCertificate;

namespace DDMS.Backend.Models.DTOs.Auth;

public class OwnerRegistrationRequest
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Phone { get; set; } = null!;

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;

    public List<VesselRegistrationItem> Vessels { get; set; } = new List<VesselRegistrationItem>();
}

public class VesselRegistrationItem
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Type { get; set; } = null!;

    public decimal? Length { get; set; }

    public decimal? Beam { get; set; }

    [Required]
    public string RegistrationNumber { get; set; } = null!;

    [Required]
    public string MooringType { get; set; } = null!;

    public DateTime? ExpectedDockingDate { get; set; }

    public List<string> RequiredServices { get; set; } = new List<string>();
    
    public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();

    public List<IFormFile> DocumentFiles { get; set; } = new List<IFormFile>();

    public List<CertificateUploadDto> Certificates { get; set; } = new List<CertificateUploadDto>();
}
