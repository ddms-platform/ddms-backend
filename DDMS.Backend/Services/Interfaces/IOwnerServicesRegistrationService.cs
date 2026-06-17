using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerServicesRegistrationService
{
    Task<TourResponse> RegisterAsync(DynamicServiceRequest request, CancellationToken ct);
}
