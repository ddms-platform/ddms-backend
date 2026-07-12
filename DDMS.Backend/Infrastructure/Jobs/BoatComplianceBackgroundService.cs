using DDMS.Backend.Configurations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Infrastructure.Jobs;

public class BoatComplianceBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BoatComplianceOptions _options;
    private readonly ILogger<BoatComplianceBackgroundService> _logger;

    public BoatComplianceBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<BoatComplianceOptions> options,
        ILogger<BoatComplianceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(_options.CheckIntervalHours);
        using var timer = new PeriodicTimer(interval);

        _logger.LogInformation(
            "Boat compliance background service started (interval: {IntervalHours}h, timezone: {TimeZoneId})",
            _options.CheckIntervalHours,
            _options.TimeZoneId);

        await RunComplianceCheckAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunComplianceCheckAsync(stoppingToken);
        }
    }

    private async Task RunComplianceCheckAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var complianceService = scope.ServiceProvider.GetRequiredService<IBoatComplianceService>();
            await complianceService.RunComplianceCheckAsync(ct);
            _logger.LogDebug("Boat compliance check completed");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Boat compliance check failed");
        }
    }
}
