using DDMS.Backend.Models.DTOs.AdminOwners;

namespace DDMS.Backend.Services.Interfaces;

public interface IAdminOwnersService
{
    Task<List<VerificationItem>> GetVerificationsAsync(CancellationToken ct);
    Task<string> ApproveVerificationAsync(Guid profileId, CancellationToken ct);
    Task<string> RejectVerificationAsync(Guid profileId, CancellationToken ct);
}
