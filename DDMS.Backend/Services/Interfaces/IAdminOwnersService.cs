using DDMS.Backend.Models.DTOs.AdminOwners;
using DDMS.Backend.Models.DTOs.OwnerDocument;

namespace DDMS.Backend.Services.Interfaces;

public interface IAdminOwnersService
{
    Task<List<VerificationItem>> GetVerificationsAsync(CancellationToken ct);
    Task<string> ApproveVerificationAsync(Guid profileId, CancellationToken ct);
    Task<string> RejectVerificationAsync(Guid profileId, CancellationToken ct);
    Task<string> ApproveDocumentsAsync(Guid profileId, CancellationToken ct);
    Task<string> RejectDocumentsAsync(Guid profileId, RejectOwnerDocumentsRequest request, CancellationToken ct);
    Task<string> ExtendDocumentDeadlineAsync(Guid profileId, ExtendOwnerDocumentDeadlineRequest request, CancellationToken ct);
    Task<string> SendDocumentReminderAsync(Guid profileId, CancellationToken ct);
}
