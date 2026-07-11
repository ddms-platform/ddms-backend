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
    private readonly BoatComplianceOptions _options;

    public BoatComplianceService(
        IBoatCertificateRepository repo,
        IBoatComplianceNotifier notifier,
        IOptions<BoatComplianceOptions> options)
    {
        _repo = repo;
        _notifier = notifier;
        _options = options.Value;
    }

    public async Task RunComplianceCheckAsync(CancellationToken ct = default)
    {
        var today = GetTodayInConfiguredTimeZone();
        var warningThreshold = today.AddDays(_options.ReminderDaysBeforeExpiry);

        await _repo.MarkExpiredAsync(today, ct);
        await SendExpiringRemindersAsync(today, warningThreshold, ct);

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
}
