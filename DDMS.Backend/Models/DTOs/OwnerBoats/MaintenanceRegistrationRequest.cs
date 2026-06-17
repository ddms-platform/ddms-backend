namespace DDMS.Backend.Models.DTOs.OwnerBoats;

public class MaintenanceRegistrationRequest
{
    public Guid serviceId { get; set; }
    public DateTime scheduledDate { get; set; }
}
