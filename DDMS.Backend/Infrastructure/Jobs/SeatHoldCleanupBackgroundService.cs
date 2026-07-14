using DDMS.Backend.Configurations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Infrastructure.Jobs;

/// <summary>
/// Worker nền: định kỳ (mặc định mỗi 1 phút) quét và tự huỷ các booking
/// đang giữ chỗ đã quá hạn (status=holding, hold_expired_at &lt;= now),
/// nhả ghế về rổ chung.
/// </summary>
public class SeatHoldCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BookingHoldOptions _options;
    private readonly ILogger<SeatHoldCleanupBackgroundService> _logger;

    public SeatHoldCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<BookingHoldOptions> options,
        ILogger<SeatHoldCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(_options.CleanupIntervalMinutes);
        using var timer = new PeriodicTimer(interval);

        _logger.LogInformation(
            "Seat hold cleanup service started (interval: {IntervalMinutes} phút)",
            _options.CleanupIntervalMinutes);

        await RunCleanupAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupAsync(stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var cancelled = await bookingService.CancelExpiredHoldsAsync(ct);
            if (cancelled > 0)
                _logger.LogInformation("Đã tự huỷ {Count} booking giữ chỗ quá hạn", cancelled);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seat hold cleanup thất bại");
        }
    }
}
