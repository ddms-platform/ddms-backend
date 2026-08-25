using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.DTOs.OwnerDocument;

namespace DDMS.Backend.Models.DTOs.AdminOwners;

public class VerificationItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Owner { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string License { get; set; } = null!;
    public string EntityType { get; set; } = "individual";
    public string Submitted { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int Boats { get; set; }
    public DateTime? DocumentUploadDeadline { get; set; }
    public bool IsDocumentDeadlineExpired { get; set; }
    public bool IsDocumentCompleted { get; set; }
    public bool IsDocumentPendingReview { get; set; }
    public bool IsDocumentApproved { get; set; }
    public bool IsDocumentRejected { get; set; }
    public bool IsDocumentResubmitted { get; set; }
    public DateTime? LastDocumentRejectedAt { get; set; }
    public DateTime? LastDocumentUpdatedAt { get; set; }
    public List<OwnerDocumentListItem> Documents { get; set; } = new();
    public List<VesselItem> Vessels { get; set; } = new();
}

public class VesselItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal? Length { get; set; }
    public decimal? Beam { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public string MooringType { get; set; } = null!;
    public string ExpectedDockingDate { get; set; } = null!;
    public List<string> RequiredServices { get; set; } = new();
    public List<string> DocumentUrls { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public List<CertificateListItem> Certificates { get; set; } = new();
    public int MaxPassengers { get; set; }
    public string Status { get; set; } = null!;
}
