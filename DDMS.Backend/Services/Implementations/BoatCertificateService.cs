using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace DDMS.Backend.Services.Implementations;

public class BoatCertificateService : IBoatCertificateService
{
    private readonly IBoatCertificateRepository _repo;
    private readonly IBoatRepository _boatRepo;
    private readonly ICloudinaryService _cloudinary;
    private readonly IBoatComplianceNotifier _notifier;
    private readonly ICertificateTypeService _certificateTypes;
    private readonly BoatComplianceOptions _options;

    public BoatCertificateService(
        IBoatCertificateRepository repo,
        IBoatRepository boatRepo,
        ICloudinaryService cloudinary,
        IBoatComplianceNotifier notifier,
        ICertificateTypeService certificateTypes,
        IOptions<BoatComplianceOptions> options)
    {
        _repo = repo;
        _boatRepo = boatRepo;
        _cloudinary = cloudinary;
        _notifier = notifier;
        _certificateTypes = certificateTypes;
        _options = options.Value;
    }

    public async Task<List<CertificateResponse>> GetByBoatIdForOwnerAsync(
        Guid boatId, Guid ownerId, CancellationToken ct = default)
    {
        await EnsureBoatOwnedAsync(boatId, ownerId, ct);
        var certs = await _repo.GetByBoatIdAsync(boatId, ct);
        return certs.Select(MapResponse).ToList();
    }

    public async Task<List<CertificateListItem>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        var certs = await _repo.GetByOwnerIdAsync(ownerId, ct);
        return certs.Select(MapListItem).ToList();
    }

    public async Task<CertificateResponse> UploadAsync(
        Guid boatId, Guid ownerId, UploadCertificateRequest request, CancellationToken ct = default)
    {
        await EnsureBoatOwnedAsync(boatId, ownerId, ct);
        await _certificateTypes.EnsureActiveCodeAsync(request.certificateType, ct);
        ValidateExpiryDate(request.expiryDate);
        ValidateFile(request.file);

        var existing = await _repo.GetByBoatAndTypeAsync(boatId, request.certificateType.Trim(), ct);
        if (existing is not null)
        {
            throw new AppException(ErrorCode.CertificateAlreadyExists, ErrorCode.Messages.CertificateAlreadyExists);
        }

        var upload = await UploadFileAsync(request.file, ct);
        var now = DateTime.UtcNow;
        var entity = new boat_certificate
        {
            id = Guid.NewGuid(),
            boat_id = boatId,
            certificate_type = request.certificateType.Trim(),
            document_url = upload.ImageUrl,
            public_id = upload.PublicId,
            expiry_date = request.expiryDate,
            status = BoatCertificateStatuses.Pending,
            created_at = now,
            updated_at = now
        };

        await _repo.AddAsync(entity, ct);
        return MapResponse(entity);
    }

    public async Task<CertificateResponse> RenewAsync(
        Guid boatId, Guid certId, Guid ownerId, RenewCertificateRequest request, CancellationToken ct = default)
    {
        await EnsureBoatOwnedAsync(boatId, ownerId, ct);
        ValidateExpiryDate(request.expiryDate);
        ValidateFile(request.file);

        var entity = await _repo.GetByIdAsync(certId, ct)
            ?? throw new NotFoundException(ErrorCode.CertificateNotFound, ErrorCode.Messages.CertificateNotFound);

        if (entity.boat_id != boatId)
        {
            throw new NotFoundException(ErrorCode.CertificateNotFound, ErrorCode.Messages.CertificateNotFound);
        }

        if (!string.IsNullOrWhiteSpace(entity.public_id))
        {
            await _cloudinary.DeleteImageAsync(entity.public_id);
        }

        var upload = await UploadFileAsync(request.file, ct);
        entity.document_url = upload.ImageUrl;
        entity.public_id = upload.PublicId;
        entity.expiry_date = request.expiryDate;
        entity.status = BoatCertificateStatuses.Pending;
        entity.rejection_reason = null;
        entity.verified_by = null;
        entity.verified_at = null;
        entity.updated_at = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, ct);
        return MapResponse(entity);
    }

    public async Task<List<CertificateListItem>> GetPendingForAdminAsync(CancellationToken ct = default)
    {
        var certs = await _repo.GetPendingForAdminAsync(ct);
        return certs.Select(MapListItem).ToList();
    }

    public async Task<List<CertificateListItem>> GetExpiringForAdminAsync(CancellationToken ct = default)
    {
        var today = GetTodayInConfiguredTimeZone();
        var threshold = today.AddDays(_options.ReminderDaysBeforeExpiry);
        var certs = await _repo.GetExpiringAsync(threshold, ct);
        return certs.Select(MapListItem).ToList();
    }

    public async Task ApproveAsync(Guid certId, Guid adminId, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(certId, ct)
            ?? throw new NotFoundException(ErrorCode.CertificateNotFound, ErrorCode.Messages.CertificateNotFound);

        if (entity.status != BoatCertificateStatuses.Pending)
        {
            throw new AppException(ErrorCode.CertificateNotApproved, ErrorCode.Messages.CertificateNotApproved);
        }

        var now = DateTime.UtcNow;
        entity.status = BoatCertificateStatuses.Approved;
        entity.verified_by = adminId;
        entity.verified_at = now;
        entity.rejection_reason = null;
        entity.updated_at = now;

        await _repo.UpdateAsync(entity, ct);
        await _notifier.NotifyCertificateApprovedAsync(entity, ct);
    }

    public async Task RejectAsync(
        Guid certId, Guid adminId, RejectCertificateRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(certId, ct)
            ?? throw new NotFoundException(ErrorCode.CertificateNotFound, ErrorCode.Messages.CertificateNotFound);

        if (entity.status != BoatCertificateStatuses.Pending)
        {
            throw new AppException(ErrorCode.CertificateNotApproved, ErrorCode.Messages.CertificateNotApproved);
        }

        if (string.IsNullOrWhiteSpace(request.reason))
        {
            throw new AppException(ErrorCode.AuthValidationFailed, ErrorCode.Messages.ValidationFailed,
                new Dictionary<string, List<string>>
                {
                    ["reason"] = ["Lý do từ chối là bắt buộc."]
                });
        }

        var now = DateTime.UtcNow;
        entity.status = BoatCertificateStatuses.Rejected;
        entity.rejection_reason = request.reason.Trim();
        entity.verified_by = adminId;
        entity.verified_at = now;
        entity.updated_at = now;

        await _repo.UpdateAsync(entity, ct);
        await _notifier.NotifyCertificateRejectedAsync(entity, ct);
    }

    public async Task UnlockBoatAsync(Guid boatId, Guid adminId, CancellationToken ct = default)
    {
        _ = adminId;

        var boat = await _repo.GetBoatByIdAsync(boatId, ct)
            ?? throw new NotFoundException(ErrorCode.BoatNotExists, ErrorCode.Messages.BoatNotExists);

        if (boat.compliance_status != BoatComplianceStatuses.Locked
            && boat.compliance_status != BoatComplianceStatuses.Hidden)
        {
            throw new AppException(ErrorCode.AuthValidationFailed, "Tàu không ở trạng thái cần mở khóa.");
        }

        var today = GetTodayInConfiguredTimeZone();
        var certs = await _repo.GetByBoatIdAsync(boatId, ct);
        var hasValidApproved = certs.Any(c =>
            c.status == BoatCertificateStatuses.Approved && c.expiry_date >= today);

        if (!hasValidApproved)
        {
            throw new AppException(ErrorCode.CertificateNotApproved, ErrorCode.Messages.CertificateNotApproved);
        }

        await _repo.SetBoatComplianceStatusAsync(boatId, BoatComplianceStatuses.Valid, ct);
    }

    private async Task EnsureBoatOwnedAsync(Guid boatId, Guid ownerId, CancellationToken ct)
    {
        var boat = await _boatRepo.GetByIdAndOwnerAsync(boatId, ownerId);
        if (boat is null)
        {
            throw new NotFoundException(ErrorCode.BoatNotExists, ErrorCode.Messages.BoatNotExists);
        }
    }

    private void ValidateExpiryDate(DateOnly expiryDate)
    {
        var today = GetTodayInConfiguredTimeZone();
        if (expiryDate <= today)
        {
            throw new AppException(ErrorCode.CertificateExpiryInvalid, ErrorCode.Messages.CertificateExpiryInvalid);
        }
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

    private DateOnly GetTodayInConfiguredTimeZone()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return DateOnly.FromDateTime(localNow);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }

    private static CertificateResponse MapResponse(boat_certificate c) => new()
    {
        id = c.id,
        boatId = c.boat_id,
        certificateType = c.certificate_type,
        documentUrl = c.document_url,
        publicId = c.public_id,
        expiryDate = c.expiry_date,
        status = c.status,
        rejectionReason = c.rejection_reason,
        verifiedBy = c.verified_by,
        verifiedAt = c.verified_at,
        createdAt = c.created_at,
        updatedAt = c.updated_at
    };

    private static CertificateListItem MapListItem(boat_certificate c) => new()
    {
        id = c.id,
        boatId = c.boat_id,
        boatName = c.boat?.name ?? "N/A",
        ownerName = c.boat?.owner?.full_name,
        certificateType = c.certificate_type,
        documentUrl = c.document_url,
        expiryDate = c.expiry_date,
        status = c.status,
        rejectionReason = c.rejection_reason,
        createdAt = c.created_at,
        updatedAt = c.updated_at
    };
}
