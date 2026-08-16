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
    public bool isReuploaded { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class OwnerDocumentsOverviewResponse
{
    public List<OwnerDocumentListItem> Documents { get; set; } = new();
    public DateTime? OwnerSince { get; set; }
    public DateTime? UploadDeadline { get; set; }
    public bool IsExpired { get; set; }
    public int DaysRemaining { get; set; }
    public int HoursRemaining { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsPendingReview { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRejected { get; set; }
    public bool IsLocked { get; set; }
    public string EntityType { get; set; } = "individual";
    public List<string> RequiredDocumentTypes { get; set; } = new();
    public List<string> MissingRequiredTypes { get; set; } = new();
}

public class ExtendOwnerDocumentDeadlineRequest
{
    public int? AdditionalDays { get; set; }
    public DateTime? NewDeadline { get; set; }
    public string? Reason { get; set; }
}

public class RejectOwnerDocumentsRequest
{
    public string? Reason { get; set; }
    public List<string>? DocumentTypes { get; set; }
}
