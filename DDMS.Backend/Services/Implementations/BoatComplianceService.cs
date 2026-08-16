using DDMS.Backend.Common.Constants;
using DDMS.Backend.Configurations;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Services.Implementations;

public class BoatComplianceService : IBoatComplianceService
{
    private readonly IBoatCertificateRepository _repo;
    private readonly IBoatComplianceNotifier _notifier;
    private readonly IOwnerDocumentRepository _ownerRepo;
    private readonly INotificationService _notificationService;
    private readonly BoatComplianceOptions _options;

    public BoatComplianceService(
        IBoatCertificateRepository repo,
        IBoatComplianceNotifier notifier,
        IOwnerDocumentRepository ownerRepo,
        INotificationService notificationService,
        IOptions<BoatComplianceOptions> options)
    {
        _repo = repo;
        _notifier = notifier;
        _ownerRepo = ownerRepo;
        _notificationService = notificationService;
        _options = options.Value;
    }

    public async Task RunComplianceCheckAsync(CancellationToken ct = default)
    {
        var today = GetTodayInConfiguredTimeZone();
        var warningThreshold = today.AddDays(_options.ReminderDaysBeforeExpiry);

        await _repo.MarkExpiredAsync(today, ct);
        await SendExpiringRemindersAsync(today, warningThreshold, ct);
        await CheckOwnerDocumentsComplianceAsync(ct);

        var boats = await _repo.GetBoatsWithCertificatesForComplianceAsync(ct);

        foreach (var boat in boats)
        {
            var previousStatus = boat.compliance_status;
            var newStatus = DetermineComplianceStatus(
                boat.boat_certificates,
                today,
                warningThreshold,
                _options.GracePeriodDays);

            if (previousStatus == newStatus)
            {
                if (newStatus is BoatComplianceStatuses.Hidden or BoatComplianceStatuses.Locked)
                {
                    await _repo.DeactivateBoatServicesAsync(boat.id, ct);
                }

                continue;
            }

            await _repo.SetBoatComplianceStatusAsync(boat.id, newStatus, ct);

            switch (newStatus)
            {
                case BoatComplianceStatuses.Hidden:
                    await _repo.DeactivateBoatServicesAsync(boat.id, ct);
                    await _notifier.NotifyCertificateExpiredHiddenAsync(boat, ct);
                    break;
                case BoatComplianceStatuses.Locked:
                    await _repo.DeactivateBoatServicesAsync(boat.id, ct);
                    if (previousStatus is not BoatComplianceStatuses.Hidden)
                    {
                        await _notifier.NotifyCertificateExpiredHiddenAsync(boat, ct);
                    }

                    await _notifier.NotifyCertificateLockedAsync(boat, ct);
                    break;
            }
        }
    }

    private async Task SendExpiringRemindersAsync(
        DateOnly today, DateOnly warningThreshold, CancellationToken ct)
    {
        var certificates = await _repo.GetCertificatesNeedingReminderAsync(today, warningThreshold, ct);

        foreach (var certificate in certificates)
        {
            await _notifier.NotifyCertificateExpiringSoonAsync(certificate, ct);
            await _repo.MarkReminderSentAsync(certificate.id, ct);
        }
    }

    internal static string DetermineComplianceStatus(
        IEnumerable<boat_certificate> certificates,
        DateOnly today,
        DateOnly warningThreshold,
        int gracePeriodDays)
    {
        // Ignore deprecated boat types (e.g. business_license → owner transport_license).
        var activeCerts = certificates
            .Where(c => !BoatCertificateTypes.IsDeprecated(c.certificate_type))
            .ToList();

        var expiredCertificates = activeCerts
            .Where(c => c.status == BoatCertificateStatuses.Expired)
            .ToList();

        if (expiredCertificates.Count > 0)
        {
            var lockDeadline = today.AddDays(-gracePeriodDays);
            if (expiredCertificates.Any(c => c.expiry_date <= lockDeadline))
            {
                return BoatComplianceStatuses.Locked;
            }

            return BoatComplianceStatuses.Hidden;
        }

        var hasApproachingExpiry = activeCerts.Any(c =>
            c.status == BoatCertificateStatuses.Approved
            && c.expiry_date >= today
            && c.expiry_date <= warningThreshold);

        return hasApproachingExpiry
            ? BoatComplianceStatuses.Warning
            : BoatComplianceStatuses.Valid;
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

    private async Task CheckOwnerDocumentsComplianceAsync(CancellationToken ct)
    {
        try
        {
            var profiles = await _ownerRepo.GetVerifiedProfilesWithDocumentsAsync(ct);
            var now = DateTime.UtcNow;

            foreach (var profile in profiles)
            {
                if (profile.document_upload_deadline.HasValue && profile.document_upload_deadline.Value.Year >= 9999)
                {
                    continue;
                }

                var deadline = profile.document_upload_deadline ?? (profile.verified_at?.AddDays(14) ?? profile.created_at.AddDays(14));
                var isExpired = now > deadline;
                var timeRemaining = !isExpired ? (deadline - now) : TimeSpan.Zero;
                var daysRemaining = (int)Math.Ceiling(timeRemaining.TotalDays);

                var requiredTypes = OwnerDocumentTypes.GetRequiredTypes(profile.entity_type).ToList();
                var uploadedTypes = (profile.owner_documents ?? new List<owner_document>())
                    .Select(d => d.document_type)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var isCompleted = requiredTypes.All(r => uploadedTypes.Contains(r));

                if (isCompleted) continue;

                if (daysRemaining == 3)
                {
                    await _notificationService.CreateNotificationAsync(
                        senderId: null,
                        type: "system",
                        title: "Hồ sơ pháp lý còn 3 ngày đến hạn ⏱️",
                        body: $"Hạn chót bổ sung giấy tờ pháp lý của bạn là ngày {deadline:dd/MM/yyyy} (còn 3 ngày). Vui lòng hoàn tất nộp giấy tờ để đảm bảo quyền mở bán tour và rút tiền.",
                        recipientIds: new List<Guid> { profile.user_id },
                        data: null,
                        ct: ct
                    );
                }
                else if (daysRemaining == 1)
                {
                    await _notificationService.CreateNotificationAsync(
                        senderId: null,
                        type: "system",
                        title: "Cảnh báo khẩn: Hồ sơ pháp lý hết hạn ngày mai ⚠️",
                        body: $"Hạn chót bổ sung giấy tờ pháp lý của bạn là ngày {deadline:dd/MM/yyyy} (hết hạn vào ngày mai). Hãy tải lên giấy tờ ngay để tránh bị tạm dừng các hoạt động kinh doanh.",
                        recipientIds: new List<Guid> { profile.user_id },
                        data: null,
                        ct: ct
                    );
                }
                else if (isExpired && (now - deadline).TotalDays <= 1)
                {
                    await _notificationService.CreateNotificationAsync(
                        senderId: null,
                        type: "system",
                        title: "Hồ sơ pháp lý đã quá thời hạn 🔒",
                        body: $"Thời hạn bổ sung giấy tờ pháp lý của bạn đã kết thúc vào ngày {deadline:dd/MM/yyyy}. Các tính năng mở bán tour và rút tiền đã tạm khóa. Vui lòng liên hệ Ban quản trị để được hỗ trợ gia hạn.",
                        recipientIds: new List<Guid> { profile.user_id },
                        data: null,
                        ct: ct
                    );
                }
            }
        }
        catch
        {
            // best-effort compliance notification
        }
    }
}
