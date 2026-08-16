using DDMS.Backend.Models.DTOs.OwnerDocument;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerDocumentService
{
    Task<OwnerDocumentsOverviewResponse> GetOverviewByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<OwnerDocumentListItem>> ListByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Uploads owner docs for registration (no SaveChanges). Returns national_id URL if uploaded.</summary>
    Task<string?> AddDocumentsOnRegisterAsync(
        Guid ownerProfileId,
        IReadOnlyList<OwnerDocumentUploadDto> documents,
        CancellationToken ct = default);

    Task<OwnerDocumentListItem> UploadOrReplaceAsync(
        Guid userId,
        UploadOwnerDocumentRequest request,
        CancellationToken ct = default);

    void ValidateRequiredDocuments(string entityType, IReadOnlyList<OwnerDocumentUploadDto> documents);
}
