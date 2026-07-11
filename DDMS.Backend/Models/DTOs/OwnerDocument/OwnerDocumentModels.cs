using Microsoft.AspNetCore.Http;

namespace DDMS.Backend.Models.DTOs.OwnerDocument;

public class OwnerDocumentUploadDto
{
    public string DocumentType { get; set; } = null!;
    public IFormFile? File { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

public class UploadOwnerDocumentRequest
{
    public string documentType { get; set; } = null!;
    public IFormFile file { get; set; } = null!;
    public DateOnly? expiryDate { get; set; }
}

public class OwnerDocumentListItem
{
    public Guid id { get; set; }
    public string documentType { get; set; } = null!;
    public string documentUrl { get; set; } = null!;
    public DateOnly? expiryDate { get; set; }
    public string? adminNote { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}
