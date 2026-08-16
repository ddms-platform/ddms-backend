using System.Text.Json;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.AdminOwners;
using DDMS.Backend.Models.DTOs.BoatCertificate;
using DDMS.Backend.Models.DTOs.OwnerDocument;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
// Cho extension IExecutionStrategy.ExecuteAsync(Func<Task>)
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Services.Implementations;

public class AdminOwnersService : IAdminOwnersService
{
    private readonly IAdminOwnersRepository _repo;
    private readonly IEmailSender _email;
    private readonly INotificationService _notificationService;

    public AdminOwnersService(
        IAdminOwnersRepository repo,
        IEmailSender email,
        INotificationService notificationService)
    {
        _repo = repo;
        _email = email;
        _notificationService = notificationService;
    }

    public async Task<List<VerificationItem>> GetVerificationsAsync(CancellationToken ct)
    {
        var profiles = await _repo.GetAllProfilesWithUserAsync(ct);
        var items = new List<VerificationItem>();
        foreach (var op in profiles)
        {
            var boatCount = await _repo.CountActiveBoatsForOwnerAsync(op.user_id, ct);
            var ownerBoats = await _repo.GetActiveBoatsWithImagesAsync(op.user_id, ct);
            items.Add(MapVerification(op, boatCount, ownerBoats));
        }
        return items;
    }

    public async Task<string> ApproveVerificationAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy yêu cầu xác thực.");

        // Ham nay co hai lan SaveChanges (tao role, roi luu phan con lai) nen
        // transaction la can that, khong bo di duoc nhu ben OwnerRegistrationService.
        //
        // Nhung tu khi Program.cs bat EnableRetryOnFailure, goi BeginTransaction
        // truc tiep se nem:
        //   The configured execution strategy 'MySqlRetryingExecutionStrategy'
        //   does not support user-initiated transactions.
        // Phai chay ca khoi qua CreateExecutionStrategy de no coi transaction la
        // mot don vi co the thu lai.
        var strategy = _repo.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _repo.BeginTransactionAsync(ct);

            var now = DateTime.UtcNow;
            profile.status = OwnerProfileStatuses.Verified;
            profile.is_verified = true;
            profile.verified_at = now;
            profile.document_upload_deadline = now.AddDays(14);
            profile.updated_at = now;

            var ownerRole = await _repo.FindRoleByNameAsync(RoleNames.Owner, ct);
            if (ownerRole == null)
            {
                ownerRole = new role { name = RoleNames.Owner, description = RoleNames.OwnerDescription };
                _repo.AddRole(ownerRole);
                await _repo.SaveChangesAsync(ct);
            }

            if (!await _repo.UserHasRoleAsync(profile.user_id, ownerRole.id, ct))
            {
                _repo.AddUserRole(new user_role
                {
                    user_id = profile.user_id,
                    role_id = ownerRole.id,
                    assigned_at = now
                });
            }

            await UpdatePendingBoatsAsync(profile.user_id, BoatStatuses.Idle, ct);

            await _repo.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        await TrySendApprovalEmailAsync(profile);

        // Gui In-App Notification den user duoc duyet
        try
        {
            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: "Hồ sơ Chủ thuyền đã được duyệt thành công 🎉",
                body: "Chúc mừng! Bạn đã chính thức trở thành Đối tác Chủ thuyền trên DDMS. Vui lòng hoàn tất tải lên các giấy tờ pháp lý cần thiết trong vòng 14 ngày.",
                recipientIds: new List<Guid> { profile.user_id },
                data: null,
                ct: ct
            );
        }
        catch { /* best effort */ }

        return "Xác thực chủ thuyền thành công.";
    }

    public async Task<string> ExtendDocumentDeadlineAsync(Guid profileId, ExtendOwnerDocumentDeadlineRequest request, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? await _repo.FindProfileAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy hồ sơ chủ thuyền.");

        var now = DateTime.UtcNow;
        DateTime newDeadline;
        if (request.NewDeadline.HasValue)
        {
            newDeadline = request.NewDeadline.Value;
        }
        else
        {
            var currentDeadline = profile.document_upload_deadline ?? profile.verified_at?.AddDays(14) ?? now;
            var baseDate = currentDeadline > now ? currentDeadline : now;
            var days = request.AdditionalDays is > 0 ? request.AdditionalDays.Value : 14;
            newDeadline = baseDate.AddDays(days);
        }

        profile.document_upload_deadline = newDeadline;
        profile.updated_at = now;
        await _repo.SaveChangesAsync(ct);

        // Send In-App Notification to owner
        try
        {
            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: "Gia hạn thời gian bổ sung giấy tờ pháp lý ⏱️",
                body: $"Hạn chót bổ sung giấy tờ pháp lý của bạn đã được gia hạn đến ngày {newDeadline:dd/MM/yyyy HH:mm}." + (string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Ghi chú: {request.Reason}"),
                recipientIds: new List<Guid> { profile.user_id },
                data: null,
                ct: ct
            );
        }
        catch { /* best effort */ }

        return $"Đã gia hạn thời hạn nộp giấy tờ đến {newDeadline:dd/MM/yyyy}.";
    }

    public async Task<string> SendDocumentReminderAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? await _repo.FindProfileAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy hồ sơ chủ thuyền.");

        var deadline = profile.document_upload_deadline ?? (profile.verified_at?.AddDays(14) ?? profile.created_at.AddDays(14));
        var isExpired = DateTime.UtcNow > deadline;

        var title = isExpired
            ? "Cảnh báo quá hạn bổ sung giấy tờ pháp lý ⚠️"
            : "Nhắc nhở hoàn tất bổ sung giấy tờ pháp lý ⏱️";

        var body = isExpired
            ? $"Hồ sơ pháp lý của bạn đã quá hạn vào ngày {deadline:dd/MM/yyyy}. Các tính năng mở bán tour và rút tiền đã tạm khóa. Vui lòng liên hệ Ban quản trị để xin gia hạn nộp hồ sơ."
            : $"Hạn chót hoàn tất bổ sung giấy tờ pháp lý của bạn là ngày {deadline:dd/MM/yyyy}. Vui lòng kiểm tra và tải lên các giấy tờ còn thiếu sớm nhất.";

        await _notificationService.CreateNotificationAsync(
            senderId: null,
            type: "system",
            title: title,
            body: body,
            recipientIds: new List<Guid> { profile.user_id },
            data: null,
            ct: ct
        );

        return "Đã gửi thông báo nhắc nhở đến chủ thuyền.";
    }

    public async Task<string> ApproveDocumentsAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? await _repo.FindProfileAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy hồ sơ chủ thuyền.");

        var requiredTypes = OwnerDocumentTypes.GetRequiredTypes(profile.entity_type ?? OwnerEntityTypes.Individual);
        var uploadedTypes = profile.owner_documents.Select(d => d.document_type).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingTypes = requiredTypes.Where(r => !uploadedTypes.Contains(r)).ToList();

        if (missingTypes.Count > 0)
        {
            throw new ValidationException($"Chủ thuyền chưa nộp đủ {requiredTypes.Count} loại giấy tờ bắt buộc (còn thiếu {missingTypes.Count} loại).");
        }

        var now = DateTime.UtcNow;
        profile.document_upload_deadline = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        profile.status = OwnerProfileStatuses.Verified;
        profile.is_verified = true;
        profile.verified_at ??= now;
        profile.updated_at = now;

        // Clear any previous rejection notes
        profile.last_document_rejected_at = null;
        foreach (var doc in profile.owner_documents)
        {
            doc.admin_note = null;
            doc.updated_at = now;
        }

        await _repo.SaveChangesAsync(ct);

        // Send In-App Notification to owner
        try
        {
            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: "Hồ sơ pháp lý đã được Ban quản trị phê duyệt 🎉",
                body: "Chúc mừng! Toàn bộ giấy tờ pháp lý của bạn đã được kiểm duyệt và chấp thuận. Tất cả các chức năng kinh doanh trên hệ thống đã được mở khóa hoàn toàn.",
                recipientIds: new List<Guid> { profile.user_id },
                data: null,
                ct: ct
            );
        }
        catch { /* best effort */ }

        return "Đã phê duyệt hồ sơ pháp lý và mở khóa toàn bộ cho chủ thuyền.";
    }

    public async Task<string> RejectDocumentsAsync(Guid profileId, RejectOwnerDocumentsRequest request, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? await _repo.FindProfileAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy hồ sơ chủ thuyền.");

        var reason = string.IsNullOrWhiteSpace(request?.Reason)
            ? "Giấy tờ chưa đạt chuẩn hoặc không hợp lệ. Vui lòng kiểm tra và cập nhật lại."
            : request.Reason.Trim();

        var now = DateTime.UtcNow;
        profile.updated_at = now;
        profile.last_document_rejected_at = now;
        if (profile.document_upload_deadline.HasValue && profile.document_upload_deadline.Value.Year >= 9999)
        {
            profile.document_upload_deadline = profile.verified_at?.AddDays(14) ?? profile.created_at.AddDays(14);
        }

        // Apply admin rejection note to targeted documents or all uploaded documents
        var targetDocs = profile.owner_documents.AsEnumerable();
        if (request?.DocumentTypes != null && request.DocumentTypes.Count > 0)
        {
            var typeSet = request.DocumentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            targetDocs = profile.owner_documents.Where(d => typeSet.Contains(d.document_type));
        }

        foreach (var doc in targetDocs)
        {
            doc.admin_note = reason;
            doc.updated_at = now;
        }

        await _repo.SaveChangesAsync(ct);

        // Send In-App Notification to owner
        try
        {
            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: "Hồ sơ pháp lý cần bổ sung / chỉnh sửa ⚠️",
                body: $"Hồ sơ pháp lý của bạn chưa được duyệt: {reason}. Vui lòng tải lên lại giấy tờ hợp lệ.",
                recipientIds: new List<Guid> { profile.user_id },
                data: null,
                ct: ct
            );
        }
        catch { /* best effort */ }

        return "Đã gửi thông báo từ chối hồ sơ pháp lý đến chủ thuyền.";
    }

    public async Task<string> RejectVerificationAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _repo.FindProfileWithUserAsync(profileId, ct)
            ?? await _repo.FindProfileAsync(profileId, ct)
            ?? throw new NotFoundException(ErrorCode.ResourceNotFound, "Không tìm thấy yêu cầu xác thực.");

        var now = DateTime.UtcNow;
        profile.status = OwnerProfileStatuses.Rejected;
        profile.is_verified = false;
        profile.updated_at = now;

        await UpdatePendingBoatsAsync(profile.user_id, BoatStatuses.Rejected, ct);

        await _repo.SaveChangesAsync(ct);

        // Gui In-App Notification den user bi tu choi
        try
        {
            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: "Hồ sơ đăng ký Chủ thuyền bị từ chối ❌",
                body: "Rất tiếc, hồ sơ đăng ký đối tác Chủ thuyền của bạn chưa được duyệt. Bạn có thể kiểm tra lại thông tin và nộp lại hồ sơ mới trên trang 'Trở thành chủ thuyền'.",
                recipientIds: new List<Guid> { profile.user_id },
                data: null,
                ct: ct
            );
        }
        catch { /* best effort */ }

        // Gui email tu choi
        await TrySendRejectionEmailAsync(profile);

        return "Đã từ chối yêu cầu xác thực.";
    }

    private async Task UpdatePendingBoatsAsync(Guid ownerId, string newStatus, CancellationToken ct)
    {
        var boats = await _repo.GetBoatsByStatusForOwnerAsync(ownerId, BoatStatuses.Pending, ct);
        var now = DateTime.UtcNow;
        foreach (var b in boats)
        {
            b.status = newStatus;
            b.updated_at = now;
        }
    }

    private async Task TrySendApprovalEmailAsync(owner_profile profile)
    {
        if (profile.user == null || string.IsNullOrEmpty(profile.user.email)) return;
        try
        {
            await _email.SendOwnerVerificationApprovedEmailAsync(
                profile.user.email,
                profile.business_name ?? profile.user.full_name ?? "Chủ thuyền");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending verification approval email: {ex.Message}");
        }
    }

    private async Task TrySendRejectionEmailAsync(owner_profile profile)
    {
        if (profile.user == null || string.IsNullOrEmpty(profile.user.email)) return;
        try
        {
            await _email.SendOwnerVerificationRejectedEmailAsync(
                profile.user.email,
                profile.business_name ?? profile.user.full_name ?? "Chủ thuyền",
                null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending verification rejection email: {ex.Message}");
        }
    }

    private static VerificationItem MapVerification(owner_profile op, int boatCount, List<boat> boats)
    {
        var requiredTypes = OwnerDocumentTypes.GetRequiredTypes(op.entity_type ?? OwnerEntityTypes.Individual);
        var uploadedTypes = op.owner_documents.Select(d => d.document_type).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasAllUploaded = requiredTypes.All(r => uploadedTypes.Contains(r));
        var hasRejectedDocs = op.owner_documents.Any(d => !string.IsNullOrWhiteSpace(d.admin_note));
        var isApproved = op.document_upload_deadline.HasValue
                         && op.document_upload_deadline.Value.Year >= 9999
                         && hasAllUploaded
                         && !hasRejectedDocs;
        DateTime? deadline = isApproved ? null : (op.document_upload_deadline ?? (op.verified_at?.AddDays(14) ?? op.created_at.AddDays(14)));
        var isExpired = !isApproved && (deadline.HasValue && DateTime.UtcNow > deadline.Value);
        var isPendingReview = hasAllUploaded && !hasRejectedDocs && !isApproved;
        var isDocumentRejected = hasRejectedDocs && !isApproved;
        var lastRejectedAt = op.last_document_rejected_at;
        var lastDocUpdatedAt = op.owner_documents.Count > 0 ? (DateTime?)op.owner_documents.Max(d => d.updated_at) : null;
        var isDocumentResubmitted = lastRejectedAt.HasValue
            && !isApproved
            && op.owner_documents.Any(d => d.updated_at > lastRejectedAt.Value && string.IsNullOrWhiteSpace(d.admin_note));

        return new()
        {
            Id = op.id,
            Name = op.business_name ?? op.user?.full_name ?? "Chủ thuyền",
            Owner = op.user?.full_name ?? "N/A",
            Email = op.user?.email ?? "N/A",
            Phone = op.phone_business ?? op.user?.phone ?? "N/A",
            Address = op.address ?? "N/A",
            License = op.license_number ?? "N/A",
            EntityType = op.entity_type ?? OwnerEntityTypes.Individual,
            Submitted = op.created_at.ToString("dd/MM/yyyy"),
            Status = (op.status ?? OwnerProfileStatuses.Pending).ToLower(),
            Boats = boatCount,
            DocumentUploadDeadline = deadline,
            IsDocumentDeadlineExpired = isExpired,
            IsDocumentCompleted = isApproved,
            IsDocumentPendingReview = isPendingReview,
            IsDocumentApproved = isApproved,
            IsDocumentRejected = isDocumentRejected,
            IsDocumentResubmitted = isDocumentResubmitted,
            LastDocumentRejectedAt = lastRejectedAt,
            LastDocumentUpdatedAt = lastDocUpdatedAt,
            Documents = op.owner_documents
                .OrderBy(d => d.document_type)
                .Select(d => new OwnerDocumentListItem
                {
                    id = d.id,
                    documentType = d.document_type,
                    documentUrl = d.document_url,
                    expiryDate = d.expiry_date,
                    adminNote = d.admin_note,
                    isReuploaded = lastRejectedAt.HasValue
                        && d.updated_at > lastRejectedAt.Value
                        && string.IsNullOrWhiteSpace(d.admin_note),
                    createdAt = d.created_at,
                    updatedAt = d.updated_at
                }).ToList(),
            Vessels = boats.Select(MapVessel).ToList()
        };
    }

    private static VesselItem MapVessel(boat b) => new()
    {
        Id = b.id,
        Name = b.name,
        Type = b.type ?? "N/A",
        Length = b.length,
        Beam = b.beam,
        RegistrationNumber = b.registration_number ?? "N/A",
        MooringType = b.mooring_type ?? "N/A",
        ExpectedDockingDate = b.expected_docking_date?.ToString("dd/MM/yyyy") ?? "N/A",
        RequiredServices = ParseJsonList(b.required_services),
        DocumentUrls = ParseJsonList(b.document_url),
        ImageUrls = b.boat_images.OrderBy(img => img.sort_order).Select(img => img.image_url).ToList(),
        Certificates = b.boat_certificates
            .OrderBy(c => c.certificate_type)
            .Select(c => new CertificateListItem
            {
                id = c.id,
                boatId = c.boat_id,
                boatName = b.name,
                certificateType = c.certificate_type,
                documentUrl = c.document_url,
                expiryDate = c.expiry_date,
                status = c.status,
                rejectionReason = c.rejection_reason,
                createdAt = c.created_at,
                updatedAt = c.updated_at
            }).ToList(),
        Status = b.status
    };

    private static List<string> ParseJsonList(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(raw) ?? new(); }
        catch { return new(); }
    }
}
