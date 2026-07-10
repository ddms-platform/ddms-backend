using Microsoft.AspNetCore.Http;

namespace DDMS.Backend.Models.DTOs.BoatCertificate;

public class UploadCertificateRequest
{
    public string certificateType { get; set; } = null!;
    public IFormFile file { get; set; } = null!;
    public DateOnly expiryDate { get; set; }
}

public class RenewCertificateRequest
{
    public IFormFile file { get; set; } = null!;
    public DateOnly expiryDate { get; set; }
}

public class RejectCertificateRequest
{
    public string reason { get; set; } = null!;
}

public class CertificateUploadDto
{
    public string CertificateType { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
    public DateOnly ExpiryDate { get; set; }
}

public class CertificateResponse
{
    public Guid id { get; set; }
    public Guid boatId { get; set; }
    public string certificateType { get; set; } = null!;
    public string documentUrl { get; set; } = null!;
    public string? publicId { get; set; }
    public DateOnly expiryDate { get; set; }
    public string status { get; set; } = null!;
    public string? rejectionReason { get; set; }
    public Guid? verifiedBy { get; set; }
    public DateTime? verifiedAt { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class CertificateListItem
{
    public Guid id { get; set; }
    public Guid boatId { get; set; }
    public string boatName { get; set; } = null!;
    public string? ownerName { get; set; }
    public string certificateType { get; set; } = null!;
    public string documentUrl { get; set; } = null!;
    public DateOnly expiryDate { get; set; }
    public string status { get; set; } = null!;
    public string? rejectionReason { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}
