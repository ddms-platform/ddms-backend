using DDMS.Backend.Data;
using DDMS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DDMS.Backend.Infrastructure.Jobs;

/// <summary>
/// Sends the daily Ops Briefing email to all admin users at 07:00 local time (UTC+7).
/// </summary>
public class OpsBriefingEmailService : BackgroundService
{
    private static readonly TimeSpan TargetLocalTime = new(7, 0, 0);
    private static readonly TimeSpan LocalOffset = TimeSpan.FromHours(7);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OpsBriefingEmailService> _logger;

    public OpsBriefingEmailService(
        IServiceScopeFactory scopeFactory,
        ILogger<OpsBriefingEmailService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ops briefing email service started (target: 07:00 VN daily).");

        while (!stoppingToken.IsCancellationRequested)
        {
            var wait = TimeUntilNextRun();
            _logger.LogInformation("Next ops briefing email in {Hours:F1}h", wait.TotalHours);
            try
            {
                await Task.Delay(wait, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ops briefing email job failed");
            }
        }
    }

    private static TimeSpan TimeUntilNextRun()
    {
        var nowLocal = DateTimeOffset.UtcNow.ToOffset(LocalOffset);
        var todayTarget = new DateTimeOffset(
            nowLocal.Year,
            nowLocal.Month,
            nowLocal.Day,
            TargetLocalTime.Hours,
            TargetLocalTime.Minutes,
            0,
            LocalOffset);
        var next = nowLocal < todayTarget ? todayTarget : todayTarget.AddDays(1);
        return next - nowLocal;
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var ops = scope.ServiceProvider.GetRequiredService<IAdminOpsService>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var briefing = await ops.GetMorningBriefingAsync(ct);

        // All users with admin role
        var admins = await db.users
            .Where(u => u.user_roles.Any(ur => ur.role.name == "admin"))
            .Select(u => new { u.email, u.full_name })
            .ToListAsync(ct);

        if (admins.Count == 0)
        {
            _logger.LogWarning("No admin users found to send briefing");
            return;
        }

        var dayVn = DateTimeOffset.UtcNow.ToOffset(LocalOffset).Date;
        int sent = 0, failed = 0;

        foreach (var admin in admins)
        {
            if (string.IsNullOrWhiteSpace(admin.email)) continue;
            try
            {
                await email.SendAdminOpsBriefingEmailAsync(
                    admin.email,
                    admin.full_name ?? "Admin",
                    dayVn,
                    briefing.Narrative,
                    briefing.Signals.ToursToday,
                    briefing.Signals.GuestsExpected,
                    briefing.Signals.RevenueForecast);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to email briefing to {Email}", admin.email);
                failed++;
            }
        }

        _logger.LogInformation("Ops briefing sent to {Sent} admins ({Failed} failed)", sent, failed);
    }
}
