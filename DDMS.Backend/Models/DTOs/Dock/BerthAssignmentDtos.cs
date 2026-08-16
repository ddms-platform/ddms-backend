namespace DDMS.Backend.Models.DTOs.Dock;

public class AssignBerthRequest
{
    /// <summary>Khoang neo, ví dụ "A12". Để trống để gỡ khoang khỏi lịch neo.</summary>
    public string? berthCode { get; set; }
}

public class BerthAssignmentResponse
{
    public Guid id { get; init; }
    public Guid dockId { get; init; }
    public Guid boatId { get; init; }
    public string? berthCode { get; init; }
}
