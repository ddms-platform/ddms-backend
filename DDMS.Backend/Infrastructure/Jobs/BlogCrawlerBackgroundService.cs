using DDMS.Backend.Configurations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DDMS.Backend.Infrastructure.Jobs;

/// <summary>
/// Quét nguồn tin theo chu kỳ và đưa bài mới lên trang.
///
/// RSS không có cơ chế đẩy, nên "tức thì" ở đây là hỏi lại nguồn mỗi
/// IntervalMinutes phút chứ không phải nhận thông báo từ báo. Chu kỳ ngắn hơn
/// vài phút không có ích: Google News cũng cần thời gian để lập chỉ mục bài mới,
/// mà mỗi lượt quét lại tốn quota Gemini.
/// </summary>
public class BlogCrawlerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BlogCrawlerOptions _options;
    private readonly ILogger<BlogCrawlerBackgroundService> _logger;

    public BlogCrawlerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<BlogCrawlerOptions> options,
        ILogger<BlogCrawlerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Worker cào tin đang tắt (BlogCrawler:Enabled = false)");
            return;
        }

        _logger.LogInformation(
            "Worker cào tin khởi động: mỗi {Interval} phút, tự đăng = {AutoPublish}",
            _options.IntervalMinutes, _options.AutoPublish);

        // Chờ một nhịp trước lượt đầu để không tranh tài nguyên lúc app vừa lên.
        try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));
        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await SafeWaitAsync(timer, stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var crawler = scope.ServiceProvider.GetRequiredService<IBlogCrawlerService>();
            var result = await crawler.CrawlAsync(ct);

            if (result.Created > 0 || result.Errors.Count > 0)
            {
                _logger.LogInformation(
                    "Cào tin: đọc {Fetched}, bỏ qua {Skipped}, tạo {Created}, lên trang {Published}, lỗi {Errors}",
                    result.Fetched, result.Skipped, result.Created, result.Published, result.Errors.Count);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            // Một lượt hỏng không được làm chết worker: lượt sau vẫn phải chạy.
            _logger.LogError(ex, "Lượt cào tin thất bại");
        }
    }

    private static async Task<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }
}
