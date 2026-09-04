namespace DDMS.Backend.Models.DTOs.OwnerServices;

public class ServiceChangeRequestResponse
{
    public Guid id { get; set; }
    public Guid tourId { get; set; }
    public string tourName { get; set; } = string.Empty;
    public string? tourStatus { get; set; }
    public decimal currentPrice { get; set; }
    public Guid boatId { get; set; }
    public string? boatName { get; set; }
    public Guid ownerId { get; set; }
    public string status { get; set; } = string.Empty;
    public string? rejectionReason { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public DynamicServiceRequest? proposed { get; set; }
}

public class RejectServiceChangeRequest
{
    public string reason { get; set; } = string.Empty;
}
