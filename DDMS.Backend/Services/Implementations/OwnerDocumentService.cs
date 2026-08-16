using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.OwnerDocument;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DDMS.Backend.Services.Implementations;

public class OwnerDocumentService : IOwnerDocumentService
{
    private readonly IOwnerDocumentRepository _repo;
    private readonly ICloudinaryService _cloudinary;
    private readonly ICertificateTypeService _certificateTypes;

    public OwnerDocumentService(
        IOwnerDocumentRepository repo,
        ICloudinaryService cloudinary,
        ICertificateTypeService certificateTypes)
    {
        _repo = repo;
        _cloudinary = cloudinary;
        _certificateTypes = certificateTypes;
    }

    public async Task<OwnerDocumentsOverviewResponse> GetOverviewByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, ct);
        var docs = await _repo.GetByProfileIdAsync(profile.id, ct);
        var now = DateTime.UtcNow;

        var ownerSince = profile.verified_at ?? profile.created_at;
        var requiredTypes = OwnerDocumentTypes.GetRequiredTypes(profile.entity_type).ToList();
        var uploadedTypes = docs.Select(d => d.document_type).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingTypes = requiredTypes.Where(r => !uploadedTypes.Contains(r)).ToList();
        var hasAllUploaded = missingTypes.Count == 0;

        // An owner's legal documents are officially approved only when Admin approves them (sets deadline to Year 9999) AND all required docs are present
        var hasRejectedDocs = docs.Any(d => !string.IsNullOrWhiteSpace(d.admin_note));
        bool isApproved = profile.document_upload_deadline.HasValue
            && profile.document_upload_deadline.Value.Year >= 9999
            && hasAllUploaded
            && !hasRejectedDocs
            && (profile.is_verified || profile.status == OwnerProfileStatuses.Verified);

        bool isExpired;
        int daysRemaining = 0;
        int hoursRemaining = 0;
        DateTime? deadline = isApproved ? null : (profile.document_upload_deadline ?? (profile.verified_at?.AddDays(14) ?? profile.created_at.AddDays(14)));

        if (isApproved)
        {
            isExpired = false;
        }
        else
        {
            isExpired = deadline.HasValue && now > deadline.Value;
            if (!isExpired && deadline.HasValue)
            {
                var timeRemaining = deadline.Value - now;
                daysRemaining = (int)timeRemaining.TotalDays;
                hoursRemaining = timeRemaining.Hours;
            }
        }

        bool isPendingReview = hasAllUploaded && !hasRejectedDocs && !isApproved;
        bool isRejected = hasRejectedDocs && !isApproved;
        bool isCompleted = isApproved;
        bool isLocked = !isApproved && (isExpired || isRejected || isPendingReview);

        return new OwnerDocumentsOverviewResponse
        {
            Documents = docs.Select(Map).ToList(),
            OwnerSince = ownerSince,
            UploadDeadline = deadline,
            IsExpired = isExpired,
            DaysRemaining = daysRemaining,
            HoursRemaining = hoursRemaining,
            IsCompleted = isCompleted,
            IsPendingReview = isPendingReview,
            IsApproved = isApproved,
            IsRejected = isRejected,
            IsLocked = isLocked,
            EntityType = profile.entity_type,
            RequiredDocumentTypes = requiredTypes,
            MissingRequiredTypes = missingTypes
        };
    }

    public async Task<List<OwnerDocumentListItem>> ListByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, ct);
        var docs = await _repo.GetByProfileIdAsync(profile.id, ct);
        return docs.Select(Map).ToList();
    }

    public async Task<string?> AddDocumentsOnRegisterAsync(
        Guid ownerProfileId,
        IReadOnlyList<OwnerDocumentUploadDto> documents,
        CancellationToken ct = default)
    {
        if (documents.Count == 0) return null;

        string? nationalIdUrl = null;
        var now = DateTime.UtcNow;
        foreach (var doc in documents)
        {
            if (doc.File is null || doc.File.Length == 0 || string.IsNullOrWhiteSpace(doc.DocumentType))
                continue;

            var type = doc.DocumentType.Trim();
            await _certificateTypes.EnsureActiveCodeAsync(type, CertificateScopes.Owner, ct);
            ValidateFile(doc.File);

            var upload = await UploadFileAsync(doc.File, ct);
            // Tracked only — caller (registration) owns the transaction / SaveChanges.
            _repo.Add(new owner_document
            {
                id = Guid.NewGuid(),
                owner_profile_id = ownerProfileId,
                document_type = type,
                document_url = upload.ImageUrl,
                public_id = upload.PublicId,
                expiry_date = doc.ExpiryDate,
                created_at = now,
                updated_at = now
            });

            if (string.Equals(type, OwnerDocumentTypes.NationalId, StringComparison.OrdinalIgnoreCase))
                nationalIdUrl = upload.ImageUrl;
        }

        return nationalIdUrl;
    }

    public async Task<OwnerDocumentListItem> UploadOrReplaceAsync(
        Guid userId,
        UploadOwnerDocumentRequest request,
        CancellationToken ct = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, ct);

        if (string.IsNullOrWhiteSpace(request.documentType))
        {
            throw new AppException(ErrorCode.OwnerDocumentTypeRequired, ErrorCode.Messages.OwnerDocumentTypeRequired);
        }

        var type = request.documentType.Trim();
        await _certificateTypes.EnsureActiveCodeAsync(type, CertificateScopes.Owner, ct);
        ValidateFile(request.file);

        var existing = await _repo.GetByProfileAndTypeAsync(profile.id, type, ct);
        var upload = await UploadFileAsync(request.file, ct);
        var now = DateTime.UtcNow;

        if (string.Equals(type, OwnerDocumentTypes.NationalId, StringComparison.OrdinalIgnoreCase))
        {
            profile.license_image = upload.ImageUrl;
            profile.updated_at = now;
        }

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(existing.public_id))
            {
                await _cloudinary.DeleteImageAsync(existing.public_id);
            }

            existing.document_url = upload.ImageUrl;
            existing.public_id = upload.PublicId;
            existing.expiry_date = request.expiryDate;
            existing.admin_note = null;
            existing.updated_at = now;
            await _repo.UpdateAsync(existing, ct);
            return Map(existing);
        }

        var entity = new owner_document
        {
            id = Guid.NewGuid(),
            owner_profile_id = profile.id,
            document_type = type,
            document_url = upload.ImageUrl,
            public_id = upload.PublicId,
            expiry_date = request.expiryDate,
            created_at = now,
            updated_at = now
        };
        await _repo.AddAsync(entity, ct);
        return Map(entity);
    }

    public void ValidateRequiredDocuments(string entityType, IReadOnlyList<OwnerDocumentUploadDto> documents)
    {
        var provided = documents
            .Where(d => d.File is { Length: > 0 } && !string.IsNullOrWhiteSpace(d.DocumentType))
            .Select(d => d.DocumentType.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = OwnerDocumentTypes.GetRequiredTypes(entityType)
            .Where(required => !provided.Contains(required))
            .ToList();

        if (missing.Count > 0)
        {
            throw new AppException(ErrorCode.OwnerDocumentRequired, ErrorCode.Messages.OwnerDocumentRequired,
                new Dictionary<string, List<string>>
                {
                    ["OwnerDocuments"] = missing.Select(m => $"Thiếu giấy tờ bắt buộc: {m}.").ToList()
                });
        }
    }

    private async Task<owner_profile> GetProfileOrThrowAsync(Guid userId, CancellationToken ct)
    {
        return await _repo.GetProfileByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy hồ sơ chủ thuyền.");
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.ValidationFailed,
                new Dictionary<string, List<string>>
                {
                    ["file"] = ["Tệp giấy tờ là bắt buộc."]
                });
        }
    }

    private async Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return await _cloudinary.UploadImageAsync(stream, file.FileName);
    }

    private static OwnerDocumentListItem Map(owner_document d) => new()
    {
        id = d.id,
        documentType = d.document_type,
        documentUrl = d.document_url,
        expiryDate = d.expiry_date,
        adminNote = d.admin_note,
        isReuploaded = d.owner_profile?.last_document_rejected_at.HasValue == true
            && d.updated_at > d.owner_profile.last_document_rejected_at.Value
            && string.IsNullOrWhiteSpace(d.admin_note),
        createdAt = d.created_at,
        updatedAt = d.updated_at
    };
}
