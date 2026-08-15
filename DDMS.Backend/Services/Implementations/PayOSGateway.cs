using DDMS.Backend.Services.Interfaces;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Services.Implementations;

public class PayOSGateway : IPayOSGateway
{
    private readonly PayOSClient _client;
    private readonly ILogger<PayOSGateway> _logger;

    public PayOSGateway(PayOSClient client, ILogger<PayOSGateway> logger)
    {
        _client = client;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_client.ClientId)
        && !string.IsNullOrWhiteSpace(_client.ApiKey)
        && !string.IsNullOrWhiteSpace(_client.ChecksumKey);

    public Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
        CreatePaymentLinkRequest request, CancellationToken ct) =>
        _client.PaymentRequests.CreateAsync(request);

    public async Task<PaymentLink?> GetPaymentLinkAsync(long orderCode, CancellationToken ct)
    {
        try
        {
            return await _client.PaymentRequests.GetAsync(orderCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không tra được đơn PayOS {OrderCode}", orderCode);
            return null;
        }
    }

    public async Task<WebhookData?> VerifyWebhookAsync(Webhook body, CancellationToken ct)
    {
        try
        {
            return await _client.Webhooks.VerifyAsync(body);
        }
        catch (Exception ex)
        {
            // Chữ ký sai thì SDK ném — với chúng ta nó tương đương "không hợp lệ".
            _logger.LogWarning(ex, "Webhook PayOS không qua được bước xác minh chữ ký");
            return null;
        }
    }
}
