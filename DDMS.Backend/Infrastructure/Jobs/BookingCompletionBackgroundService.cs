using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Infrastructure.Jobs;

/// <summary>
/// Đánh dấu hoàn thành những đơn đã trả tiền của chuyến đã chạy xong.
///
/// Trước khi có worker này, không chỗ nào trong toàn hệ thống ghi trạng thái
/// "completed", trong khi ReviewRepository lại lọc đúng nó — nên không khách nào
/// từng đánh giá được tour. Đây là mảnh còn thiếu của vòng đời một booking.
///
/// Chạy mỗi giờ là đủ: không ai cần được mời đánh giá ngay giây tour vừa cập bến,
/// và quét thưa thì mỗi lần chỉ là một câu UPDATE có điều kiện.
/// </summary>
public class BookingCompletionBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCompletionBackgroundService> _logger;

    public BookingCompletionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingCompletionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        _logger.LogInformation(
            "Booking completion service started (interval: {IntervalHours} giờ)", Interval.TotalHours);

        await RunAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunAsync(stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bookings = scope.ServiceProvider.GetRequiredService<IBookingService>();

            var completed = await bookings.CompleteFinishedToursAsync(ct);
            if (completed > 0)
                _logger.LogInformation("Đã đánh dấu hoàn thành {Count} booking", completed);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Đánh dấu hoàn thành booking thất bại");
        }
    }
}
